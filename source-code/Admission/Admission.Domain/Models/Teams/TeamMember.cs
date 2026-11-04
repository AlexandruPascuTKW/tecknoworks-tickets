namespace Admission.Domain.Models.Teams;

/// <summary>
/// Admission's own copy of who is on which Organizer's Team, kept up to date from Accounts' events
/// (ADR-001). ADM-R1 is checked against this, never against Accounts.
/// </summary>
public class TeamMember : Entity, IAggregateRoot
{
    internal TeamMember(Guid organizerId, Guid accountId, TeamRole teamRole)
    {
        OrganizerId = organizerId;
        AccountId = accountId;
        TeamRole = teamRole;
    }

    private TeamMember()
    {
        TeamRole = default!;
    }

    public Guid OrganizerId { get; private set; }

    public Guid AccountId { get; private set; }

    public TeamRole TeamRole { get; private set; }

    public TeamMember ChangeRole(TeamRole teamRole)
    {
        TeamRole = teamRole;
        return this;
    }
}
