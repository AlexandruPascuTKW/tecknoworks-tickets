namespace Admission.Domain.Models.Performances;

/// <summary>
/// Admission's own copy of a published Performance — what "My tickets" and the Door list show about it —
/// kept up to date from Events' snapshots (ADR-005).
/// </summary>
public class Performance : Entity, IAggregateRoot
{
    internal Performance(
        Guid performanceId,
        Guid eventId,
        Guid organizerId,
        string eventName,
        string venueName,
        DateOnly date,
        TimeOnly doorsTime,
        TimeOnly startTime)
    {
        PerformanceId = performanceId;
        EventId = eventId;
        OrganizerId = organizerId;
        EventName = eventName;
        VenueName = venueName;
        Date = date;
        DoorsTime = doorsTime;
        StartTime = startTime;
    }

    private Performance()
    {
        EventName = default!;
        VenueName = default!;
    }

    /// <summary>As Events publishes it.</summary>
    public Guid PerformanceId { get; private set; }

    public Guid EventId { get; private set; }

    public Guid OrganizerId { get; private set; }

    public string EventName { get; private set; }

    public string VenueName { get; private set; }

    public DateOnly Date { get; private set; }

    public TimeOnly DoorsTime { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public Performance Apply(string eventName, string venueName, DateOnly date, TimeOnly doorsTime, TimeOnly startTime)
    {
        EventName = eventName;
        VenueName = venueName;
        Date = date;
        DoorsTime = doorsTime;
        StartTime = startTime;
        return this;
    }
}
