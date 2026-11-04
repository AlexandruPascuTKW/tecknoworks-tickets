namespace Events.Domain.Models.Events;

/// <summary>One dated occurrence of the Event at its Venue (EVT glossary).</summary>
public class Performance : Entity
{
    internal Performance(PerformanceSchedule schedule) => Schedule = schedule;

    private Performance()
    {
        Schedule = default!;
    }

    public PerformanceSchedule Schedule { get; internal set; }
}
