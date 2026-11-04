using Sales.Domain.Models.Performances;

namespace Sales.Domain.Factories;

/// <summary>Builds Sales' view of a Performance from what Events published.</summary>
internal class PerformanceFactory : IPerformanceFactory
{
    private readonly List<OfferedTicketTypeData> ticketTypes = new();
    private Guid? performanceId;
    private Guid eventId;
    private Guid organizerId;
    private string? eventName;
    private DateTime startsAt;

    public IPerformanceFactory ForPerformance(
        Guid performanceId,
        Guid eventId,
        Guid organizerId,
        string eventName,
        DateTime startsAt)
    {
        ticketTypes.Clear(); // starting a new Performance: the factory may be reused for several
        this.performanceId = performanceId;
        this.eventId = eventId;
        this.organizerId = organizerId;
        this.eventName = eventName;
        this.startsAt = startsAt;
        return this;
    }

    public IPerformanceFactory WithTicketType(OfferedTicketTypeData ticketType)
    {
        ticketTypes.Add(ticketType);
        return this;
    }

    public Performance Build()
    {
        if (performanceId is null || eventName is null)
        {
            throw new InvalidOperationException("A Performance needs an id and an Event name.");
        }

        var performance = new Performance(performanceId.Value, eventId, organizerId, eventName, startsAt);

        return performance.Apply(eventName, startsAt, ticketTypes);
    }
}
