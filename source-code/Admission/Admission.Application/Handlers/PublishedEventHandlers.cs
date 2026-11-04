using Admission.Domain.Factories;
using Admission.Domain.Repositories;

namespace Admission.Application.Handlers;

/// <summary>
/// Turns what Events published into Admission's own Performances (ADR-005). Both published-Event events
/// carry the whole snapshot, so this replaces Admission's copy — safe to run more than once.
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
        string venueName,
        PublishedPerformance[] published)
    {
        var existing = await performances.ForEvent(eventId);

        foreach (var performance in published)
        {
            var found = existing.FirstOrDefault(p => p.PerformanceId == performance.PerformanceId);

            if (found is not null)
            {
                await performances.Save(found.Apply(
                    eventName, venueName, performance.Date, performance.DoorsTime, performance.StartTime));
                continue;
            }

            await performances.Save(factory
                .ForPerformance(
                    performance.PerformanceId,
                    eventId,
                    organizerId,
                    eventName,
                    venueName,
                    performance.Date,
                    performance.DoorsTime,
                    performance.StartTime)
                .Build());
        }

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
        => synchronizer.Sync(e.EventId, e.OrganizerId, e.EventName, e.VenueName, e.Performances);
}

public class PublishedEventUpdatedEventHandler : IEventHandler<PublishedEventUpdatedEvent>
{
    private readonly PerformanceSynchronizer synchronizer;

    public PublishedEventUpdatedEventHandler(PerformanceSynchronizer synchronizer) => this.synchronizer = synchronizer;

    public Task Handle(PublishedEventUpdatedEvent e)
        => synchronizer.Sync(e.EventId, e.OrganizerId, e.EventName, e.VenueName, e.Performances);
}
