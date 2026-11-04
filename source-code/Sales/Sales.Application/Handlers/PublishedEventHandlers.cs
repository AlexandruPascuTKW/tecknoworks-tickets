using Sales.Domain.Factories;
using Sales.Domain.Models.Performances;
using Sales.Domain.Repositories;

namespace Sales.Application.Handlers;

/// <summary>
/// Turns what Events published into Sales' own Performances (ADR-005). Both published-Event events carry
/// the whole snapshot, so this replaces Sales' copy — which makes it safe to run more than once.
/// </summary>
public class PerformanceSynchronizer
{
    private readonly IPerformanceDomainRepository performances;
    private readonly IPerformanceFactory factory;

    public PerformanceSynchronizer(IPerformanceDomainRepository performances, IPerformanceFactory factory)
    {
        this.performances = performances;
        this.factory = factory;
    }

    public async Task Sync(
        Guid eventId,
        Guid organizerId,
        string eventName,
        PublishedPerformance[] published,
        PublishedTicketType[] ticketTypes)
    {
        var types = ticketTypes
            .Select(t => new OfferedTicketTypeData(t.TicketTypeId, t.Name, t.Price, t.Quantity, t.OnSale))
            .ToList();

        var existing = await performances.ForEvent(eventId);

        foreach (var performance in published)
        {
            var startsAt = performance.Date.ToDateTime(performance.StartTime);
            var found = existing.FirstOrDefault(p => p.PerformanceId == performance.PerformanceId);

            if (found is not null)
            {
                await performances.Save(found.Apply(eventName, startsAt, types));
                continue;
            }

            factory.ForPerformance(performance.PerformanceId, eventId, organizerId, eventName, startsAt);

            foreach (var type in types)
            {
                factory.WithTicketType(type);
            }

            await performances.Save(factory.Build());
        }

        // EVT-R8: a Performance with sales is never removed, so what is gone here has nothing to lose.
        foreach (var gone in existing.Where(e => published.All(p => p.PerformanceId != e.PerformanceId)))
        {
            await performances.Delete(gone);
        }
    }

}

public class EventPublishedEventHandler : IEventHandler<EventPublishedEvent>
{
    private readonly PerformanceSynchronizer synchronizer;

    public EventPublishedEventHandler(PerformanceSynchronizer synchronizer) => this.synchronizer = synchronizer;

    public Task Handle(EventPublishedEvent e)
        => synchronizer.Sync(e.EventId, e.OrganizerId, e.EventName, e.Performances, e.TicketTypes);
}

public class PublishedEventUpdatedEventHandler : IEventHandler<PublishedEventUpdatedEvent>
{
    private readonly PerformanceSynchronizer synchronizer;

    public PublishedEventUpdatedEventHandler(PerformanceSynchronizer synchronizer) => this.synchronizer = synchronizer;

    public Task Handle(PublishedEventUpdatedEvent e)
        => synchronizer.Sync(e.EventId, e.OrganizerId, e.EventName, e.Performances, e.TicketTypes);
}
