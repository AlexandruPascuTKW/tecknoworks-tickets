using Sales.Application.Orders.Commands.RecordPaymentResult;
using Sales.Application.Services;

namespace Sales.Application.Orders.Commands.SimulatedPayment;

/// <summary>ADR-004 — what the simulated provider's page shows about a payment.</summary>
public class SimulatedPaymentQuery : IRequest<SimulatedPaymentInfo>
{
    public string ProviderReference { get; set; } = string.Empty;

    public class SimulatedPaymentQueryHandler : IRequestHandler<SimulatedPaymentQuery, SimulatedPaymentInfo>
    {
        private readonly ISimulatedPaymentPages pages;

        public SimulatedPaymentQueryHandler(ISimulatedPaymentPages pages) => this.pages = pages;

        public Task<SimulatedPaymentInfo> Handle(SimulatedPaymentQuery request, CancellationToken cancellationToken)
            => Task.FromResult(
                pages.Find(request.ProviderReference)
                ?? throw new NotFoundException("Payment", request.ProviderReference));
    }
}

/// <summary>
/// ADR-004 — the Attendee pays with an approving or a declining test card on the simulated page. The
/// simulated provider then notifies us through the same command a real provider's callback would send.
/// </summary>
public class CompleteSimulatedPaymentCommand : IRequest<SimulatedPaymentInfo>
{
    public string ProviderReference { get; set; } = string.Empty;

    public bool Approve { get; set; }

    public class CompleteSimulatedPaymentCommandHandler
        : IRequestHandler<CompleteSimulatedPaymentCommand, SimulatedPaymentInfo>
    {
        private readonly ISimulatedPaymentPages pages;
        private readonly IMediator mediator;

        public CompleteSimulatedPaymentCommandHandler(ISimulatedPaymentPages pages, IMediator mediator)
        {
            this.pages = pages;
            this.mediator = mediator;
        }

        public async Task<SimulatedPaymentInfo> Handle(
            CompleteSimulatedPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var notification = pages.Complete(request.ProviderReference, request.Approve);

            await mediator.Send(
                new RecordPaymentResultCommand
                {
                    ProviderReference = notification.ProviderReference,
                    Outcome = notification.Result,
                    Signature = notification.Signature,
                },
                cancellationToken);

            return pages.Find(request.ProviderReference)!;
        }
    }
}
