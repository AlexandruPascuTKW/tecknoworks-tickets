namespace Sales.Application.Settings;

/// <summary>ADR-002 to ADR-004, ADR-006. Bound from the "Sales" configuration section.</summary>
public class SalesSettings
{
    /// <summary>IANA id of the time zone all Venues are in (the same one as in the Events settings).</summary>
    public string TimeZone { get; set; } = "Europe/Dublin";

    /// <summary>ADR-003: how often lapsed Holds are marked Expired.</summary>
    public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>ADR-002: how many times a checkout that lost a race tries again.</summary>
    public int CheckoutAttempts { get; set; } = 5;

    /// <summary>ADR-004: signs the simulated provider's notifications. Development only.</summary>
    public string SimulatedProviderKey { get; set; } = "development-only-simulated-provider-key";
}
