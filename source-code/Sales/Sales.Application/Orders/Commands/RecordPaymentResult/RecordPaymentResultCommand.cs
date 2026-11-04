using Sales.Application.Common;
using Sales.Application.Services;
using Sales.Domain.Exceptions;
using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands.RecordPaymentResult;

/// <summary>
/// The Payment provider telling us how a payment ended (SAL-R12, SAL-R13). Paid while the Hold lasts
/// confirms the Order; Paid after it ran out confirms it only if the same tickets are still free, and
/// otherwise refunds the whole Payment automatically. Providers repeat notifications, so a payment that
/// already has a result changes nothing.
/// </summary>
public class RecordPaymentResultCommand : IRequest<Result>
{
    public string ProviderReference { get; set; } = string.Empty;

    /// <summary>"Paid" or "Declined".</summary>
    public string Outcome { get; set; } = string.Empty;

    public string Signature { get; set; } = string.Empty;

    public class RecordPaymentResultCommandHandler : IRequestHandler<RecordPaymentResultCommand, Result>
    {
        private readonly IOrderDomainRepository orders;
        private readonly IPerformanceDomainRepository performances;
        private readonly IPaymentProvider provider;
        private readonly SalesClock clock;

        public RecordPaymentResultCommandHandler(
            IOrderDomainRepository orders,
            IPerformanceDomainRepository performances,
            IPaymentProvider provider,
            SalesClock clock)
        {
            this.orders = orders;
            this.performances = performances;
            this.provider = provider;
            this.clock = clock;
        }

        public async Task<Result> Handle(RecordPaymentResultCommand request, CancellationToken cancellationToken)
        {
            if (!provider.Verify(new PaymentNotification(request.ProviderReference, request.Outcome, request.Signature)))
            {
                throw new ForbiddenException("This notification did not come from the payment provider.");
            }

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await Record(request, cancellationToken);
                    return Result.Success;
                }
                catch (ConcurrencyConflictException) when (attempt < 5)
                {
                    await Task.Delay(Random.Shared.Next(10, 50), cancellationToken);
                }
            }
        }

        private async Task Record(RecordPaymentResultCommand request, CancellationToken cancellationToken)
        {
            var order = await orders.FindByPaymentReference(request.ProviderReference, cancellationToken)
                        ?? throw new NotFoundException("Payment", request.ProviderReference);

            if (order.PaymentFor(request.ProviderReference)!.Result is not null)
            {
                return; // already recorded
            }

            if (request.Outcome == PaymentResult.Declined.Name)
            {
                // SAL-R12: the Order stays Pending payment, to be paid again until its Hold ends.
                order.RecordDeclined(request.ProviderReference);
                await orders.Save(order, cancellationToken);
                return;
            }

            var now = clock.UtcNow;

            if (order.Status == OrderStatus.PendingPayment && !order.HasLapsed(now))
            {
                // SAL-R12: Paid while the Hold lasts.
                order.ConfirmOnTime(request.ProviderReference, now);
                await orders.Save(order, cancellationToken);
                return;
            }

            if (order.Status == OrderStatus.Confirmed)
            {
                return; // nothing in the specs for a second payment on a Confirmed Order
            }

            // SAL-R13: Paid after the Hold ran out.
            var performance = await performances.FindByPerformanceId(order.PerformanceId, cancellationToken);
            var lines = order.Lines.Select(l => (l.TicketTypeId, l.NumberOfTickets)).ToList();

            var active = await orders.ActiveTicketCounts(order.PerformanceId, now, [order.Id], cancellationToken);

            if (performance is not null && performance.TryTakeBack(lines, active))
            {
                order.ConfirmLate(request.ProviderReference);
            }
            else
            {
                await provider.Refund(request.ProviderReference, cancellationToken);
                order.MarkRefunded(request.ProviderReference);
            }

            await orders.Save(order, cancellationToken);
        }
    }
}
