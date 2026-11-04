namespace Accounts.Application.Settings;

/// <summary>ADR-001 (token lifetime), ADR-007 (invitations). Bound from the "Accounts" configuration section.</summary>
public class AccountsSettings
{
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(8);

    public TimeSpan InvitationLifetime { get; set; } = TimeSpan.FromDays(7);

    /// <summary>The frontend page where someone chooses their Password; the token is appended as ?token=.</summary>
    public string InvitationLinkBase { get; set; } = "http://localhost:5173/choose-password";

    /// <summary>
    /// TecknoworksTickets staff. No use case in the specs creates Platform admins or the Finance manager,
    /// so they are seeded from configuration.
    /// </summary>
    public List<SeededStaffMember> SeededStaff { get; set; } = new();
}

public class SeededStaffMember
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PlatformRole { get; set; } = string.Empty;
}
