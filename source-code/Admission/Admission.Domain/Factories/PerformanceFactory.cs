using Admission.Domain.Models.Performances;

namespace Admission.Domain.Factories;

internal class PerformanceFactory : IPerformanceFactory
{
    private Guid? performanceId;
    private Guid eventId;
    private Guid organizerId;
    private string? eventName;
    private string? venueName;
    private DateOnly date;
    private TimeOnly doorsTime;
    private TimeOnly startTime;

    public IPerformanceFactory ForPerformance(
        Guid performanceId,
        Guid eventId,
        Guid organizerId,
        string eventName,
        string venueName,
        DateOnly date,
        TimeOnly doorsTime,
        TimeOnly startTime)
    {
        this.performanceId = performanceId;
        this.eventId = eventId;
        this.organizerId = organizerId;
        this.eventName = eventName;
        this.venueName = venueName;
        this.date = date;
        this.doorsTime = doorsTime;
        this.startTime = startTime;
        return this;
    }

    public Performance Build()
    {
        if (performanceId is null || eventName is null || venueName is null)
        {
            throw new InvalidOperationException("A Performance needs an id, an Event name and a Venue name.");
        }

        return new Performance(performanceId.Value, eventId, organizerId, eventName, venueName, date, doorsTime, startTime);
    }
}
