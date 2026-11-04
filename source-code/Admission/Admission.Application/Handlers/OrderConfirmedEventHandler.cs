using Admission.Domain.Factories;
using Admission.Domain.Repositories;

namespace Admission.Application.Handlers;

/// <summary>
/// ADM-R2 — when an Order becomes Confirmed, one Ticket is issued for every ticket on it, each with a code of
/// its own. Delivery is at least once (ADR-005), so an Order that already has Tickets issues nothing again.
/// </summary>
public class OrderConfirmedEventHandler : IEventHandler<OrderConfirmedEvent>
{
    private readonly ITicketDomainRepository tickets;
    private readonly ITicketFactory factory;

    public OrderConfirmedEventHandler(ITicketDomainRepository tickets, ITicketFactory factory)
    {
        this.tickets = tickets;
        this.factory = factory;
    }

    public async Task Handle(OrderConfirmedEvent domainEvent)
    {
        if (await tickets.ExistsForOrder(domainEvent.OrderId))
        {
            return;
        }

        factory
            .ForOrder(domainEvent.OrderId, domainEvent.PerformanceId, domainEvent.OrganizerId)
            .ForBuyer(domainEvent.BuyerAccountId, domainEvent.BuyerName);

        var issued = domainEvent.Lines
            .SelectMany(line => Enumerable
                .Range(0, line.NumberOfTickets)
                .Select(_ => factory.OfType(line.TicketTypeId, line.TicketTypeName).Build()))
            .ToList();

        await tickets.SaveAll(issued);
    }
}
