namespace Accounts.Domain.Models.Organizers;

/// <summary>An Account on an Organizer's Team, with a Team role there (ACC-R1).</summary>
public class TeamMember : Entity
{
    internal TeamMember(Guid accountId, TeamRole teamRole)
    {
        AccountId = accountId;
        TeamRole = teamRole;
    }

    private TeamMember()
    {
        TeamRole = default!;
    }

    public Guid AccountId { get; private set; }

    public TeamRole TeamRole { get; internal set; }
}
