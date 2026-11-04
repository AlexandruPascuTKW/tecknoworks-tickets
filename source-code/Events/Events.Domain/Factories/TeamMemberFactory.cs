using Events.Domain.Models.Teams;

namespace Events.Domain.Factories;

internal class TeamMemberFactory : ITeamMemberFactory
{
    private Guid? organizerId;
    private Guid? accountId;
    private TeamRole? teamRole;

    public ITeamMemberFactory OnTeamOf(Guid organizerId)
    {
        this.organizerId = organizerId;
        return this;
    }

    public ITeamMemberFactory ForAccount(Guid accountId)
    {
        this.accountId = accountId;
        return this;
    }

    public ITeamMemberFactory WithTeamRole(TeamRole teamRole)
    {
        this.teamRole = teamRole;
        return this;
    }

    public TeamMember Build()
    {
        if (organizerId is null || accountId is null || teamRole is null)
        {
            throw new InvalidOperationException("A Team member needs an Organizer, an Account and a Team role.");
        }

        return new TeamMember(organizerId.Value, accountId.Value, teamRole);
    }
}
