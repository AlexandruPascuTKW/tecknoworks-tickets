using Microsoft.Extensions.Options;
using Sales.Application.Settings;

namespace Sales.Application.Common;

/// <summary>ADR-006: instants are UTC; "still to come" is judged in the one time zone all Venues are in.</summary>
public class SalesClock
{
    private readonly TimeProvider clock;
    private readonly TimeZoneInfo zone;

    public SalesClock(TimeProvider clock, IOptions<SalesSettings> settings)
    {
        this.clock = clock;
        zone = TimeZoneInfo.FindSystemTimeZoneById(settings.Value.TimeZone);
    }

    public DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    public DateTime NowAtVenue => TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime;
}
