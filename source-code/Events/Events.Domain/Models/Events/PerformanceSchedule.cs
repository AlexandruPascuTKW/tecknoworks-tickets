using Events.Domain.Exceptions;

namespace Events.Domain.Models.Events;

/// <summary>EVT-R7 — local time at the Venue; the Doors time is at or before the Start time.</summary>
public class PerformanceSchedule : ValueObject
{
    private PerformanceSchedule()
    {
    }

    public PerformanceSchedule(DateOnly date, TimeOnly doorsTime, TimeOnly startTime)
    {
        if (doorsTime > startTime)
        {
            throw new InvalidEventException("Doors time must be at or before the start time.");
        }

        Date = date;
        DoorsTime = doorsTime;
        StartTime = startTime;
    }

    public DateOnly Date { get; private set; }

    public TimeOnly DoorsTime { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public DateTime StartsAt => Date.ToDateTime(StartTime);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Date;
        yield return DoorsTime;
        yield return StartTime;
    }
}
