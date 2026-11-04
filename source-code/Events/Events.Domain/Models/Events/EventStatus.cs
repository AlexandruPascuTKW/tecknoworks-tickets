namespace Events.Domain.Models.Events;

/// <summary>EVT-F8 — Draft or Published.</summary>
public class EventStatus : Enumeration
{
    public static readonly EventStatus Draft = new(1, nameof(Draft));
    public static readonly EventStatus Published = new(2, nameof(Published));

    private EventStatus(int value, string name) : base(value, name)
    {
    }
}
