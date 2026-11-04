using Events.Application.Settings;
using Microsoft.Extensions.Options;

namespace Events.Application.Common;

/// <summary>EVT-R7 / ADR-006: "now" as it is at the Venues, in the one configured time zone.</summary>
public class VenueTime
{
    private readonly TimeProvider clock;
    private readonly TimeZoneInfo zone;

    public VenueTime(TimeProvider clock, IOptions<EventsSettings> settings)
    {
        this.clock = clock;
        zone = TimeZoneInfo.FindSystemTimeZoneById(settings.Value.TimeZone);
    }

    public DateTime Now() => TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime;
}
