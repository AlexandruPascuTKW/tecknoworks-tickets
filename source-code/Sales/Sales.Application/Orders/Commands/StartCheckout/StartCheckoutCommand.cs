using Microsoft.Extensions.Options;
using Sales.Application.Common;
using Sales.Application.Settings;
using Sales.Domain.Exceptions;
using Sales.Domain.Factories;
using Sales.Domain.Models.Performances;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands.StartCheckout;

public class TicketChoice
{
    public Guid TicketTypeId { get; set; }

    public int NumberOfTickets { get; set; }
}

/// <summary>
/// SAL-A1 — Start checkout. Places the Order and holds its tickets for ten minutes. Everything that
/// changes what the Performance can sell is saved in one go, guarded by the Performance's Version (ADR-002).
/// </summary>
public class StartCheckoutCommand : IRequest<StartCheckoutResponse>
{
    public Guid PerformanceId { get; set; }

    public List<TicketChoice> Lines { get; set; } = new();

    public class StartCheckoutCommandHandler : IRequestHandler<StartCheckoutCommand, StartCheckoutResponse>
    {
        private readonly IPerformanceDomainRepository performances;
        private readonly IOrderDomainRepository orders;
        private readonly IOrderFactory factory;
        private readonly ICurrentUser user;
        private readonly SalesClock clock;
        private readonly SalesSettings settings;

        public StartCheckoutCommandHandler(
            IPerformanceDomainRepository performances,
            IOrderDomainRepository orders,
            IOrderFactory factory,
            ICurrentUser user,
            SalesClock clock,
            IOptions<SalesSettings> settings)
        {
            this.performances = performances;
            this.orders = orders;
            this.factory = factory;
            this.user = user;
            this.clock = clock;
            this.settings = settings.Value;
        }

        public async Task<StartCheckoutResponse> Handle(StartCheckoutCommand request, CancellationToken cancellationToken)
        {
            var buyerId = user.UserId ?? throw new ForbiddenException("Sign in to buy tickets.");

            // Choosing the same Ticket type twice is one Order line.
            var lines = request.Lines
                .GroupBy(l => l.TicketTypeId)
                .Select(g => (TicketTypeId: g.Key, NumberOfTickets: g.Sum(l => l.NumberOfTickets)))
                .ToList();

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await Attempt(request, buyerId, lines, cancellationToken);
                }
                catch (ConcurrencyConflictException) when (attempt < settings.CheckoutAttempts)
                {
                    // Someone else's checkout for this Performance won the race; look again.
                    await Task.Delay(Random.Shared.Next(10, 50), cancellationToken);
                }
                catch (ConcurrencyConflictException)
                {
                    throw new CheckoutRefusedException("A lot of people are buying at once. Please try again.");
                }
            }
        }

        private async Task<StartCheckoutResponse> Attempt(
            StartCheckoutCommand request,
            Guid buyerId,
            List<(Guid TicketTypeId, int NumberOfTickets)> lines,
            CancellationToken cancellationToken)
        {
            // SAL-R3: a Performance whose Event is not Published is not known to Sales at all.
            var performance = await performances.FindByPerformanceId(request.PerformanceId, cancellationToken)
                              ?? throw new NotFoundException(nameof(Performance), request.PerformanceId);

            var now = clock.UtcNow;

            // SAL-R15: a new checkout replaces the Attendee's earlier one for this Performance.
            var earlier = await orders.PendingFor(buyerId, request.PerformanceId, cancellationToken);
            foreach (var order in earlier)
            {
                order.Expire();
            }

            // SAL-R6: what is sold or held, leaving out the Order this one replaces.
            var active = await orders.ActiveTicketCounts(
                request.PerformanceId,
                now,
                earlier.Select(o => o.Id).ToList(),
                cancellationToken);

            performance.Reserve(lines, active, clock.NowAtVenue);

            factory
                .ForBuyer(buyerId, user.Name ?? string.Empty, user.Email ?? string.Empty)
                .ForPerformance(performance)
                .PlacedAt(now);

            foreach (var (ticketTypeId, numberOfTickets) in lines)
            {
                factory.WithLine(ticketTypeId, numberOfTickets);
            }

            var newOrder = factory.Build();

            // One SaveChanges: the new Order, the replaced Orders and the Performance's new Version.
            await orders.Save(newOrder, cancellationToken);

            return new StartCheckoutResponse(newOrder.Id);
        }
    }
}

public record StartCheckoutResponse(Guid OrderId);
