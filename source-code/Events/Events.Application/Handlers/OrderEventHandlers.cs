using Events.Domain.Factories;
using Events.Domain.Repositories;

namespace Events.Application.Handlers;

// EVT-R8: "has sales" means tickets on a Confirmed or Pending payment Order. Events learns that from
// Sales' events and never asks Sales. Each handler is idempotent (ADR-005).

public class OrderPlacedEventHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly ISaleDomainRepository sales;
    private readonly ISaleFactory factory;

    public OrderPlacedEventHandler(ISaleDomainRepository sales, ISaleFactory factory)
    {
        this.sales = sales;
        this.factory = factory;
    }

    public async Task Handle(OrderPlacedEvent domainEvent)
    {
        if (await sales.Find(domainEvent.OrderId) is not null)
        {
            return;
        }

        factory.ForOrder(domainEvent.OrderId, domainEvent.PerformanceId);

        foreach (var line in domainEvent.Lines)
        {
            factory.WithLine(line.TicketTypeId, line.NumberOfTickets);
        }

        await sales.Save(factory.Build());
    }
}

public class OrderConfirmedEventHandler : IEventHandler<OrderConfirmedEvent>
{
    private readonly ISaleDomainRepository sales;
    private readonly ISaleFactory factory;

    public OrderConfirmedEventHandler(ISaleDomainRepository sales, ISaleFactory factory)
    {
        this.sales = sales;
        this.factory = factory;
    }

    public async Task Handle(OrderConfirmedEvent domainEvent)
    {
        var existing = await sales.Find(domainEvent.OrderId);

        if (existing is not null)
        {
            // A late Payment can confirm an Order that had Expired (SAL-R13).
            await sales.Save(existing.Reinstate());
            return;
        }

        factory.ForOrder(domainEvent.OrderId, domainEvent.PerformanceId);

        foreach (var line in domainEvent.Lines)
        {
            factory.WithLine(line.TicketTypeId, line.NumberOfTickets);
        }

        await sales.Save(factory.Build());
    }
}

public class OrderExpiredEventHandler : IEventHandler<OrderExpiredEvent>
{
    private readonly ISaleDomainRepository sales;

    public OrderExpiredEventHandler(ISaleDomainRepository sales) => this.sales = sales;

    public async Task Handle(OrderExpiredEvent domainEvent)
    {
        var existing = await sales.Find(domainEvent.OrderId);

        if (existing is not null)
        {
            await sales.Save(existing.Release());
        }
    }
}
