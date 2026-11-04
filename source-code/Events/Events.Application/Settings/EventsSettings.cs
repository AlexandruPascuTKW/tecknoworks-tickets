namespace Events.Application.Settings;

/// <summary>ADR-006: one time zone for every Venue. Bound from the "Events" configuration section.</summary>
public class EventsSettings
{
    /// <summary>IANA id of the time zone all Venues are in; "still to come" (EVT-R7) is judged against it.</summary>
    public string TimeZone { get; set; } = "Europe/Dublin";
}
