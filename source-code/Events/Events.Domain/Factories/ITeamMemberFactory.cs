using Events.Domain.Models.Teams;

namespace Events.Domain.Factories;

public interface ITeamMemberFactory : IFactory<TeamMember>
{
    ITeamMemberFactory OnTeamOf(Guid organizerId);
    ITeamMemberFactory ForAccount(Guid accountId);
    ITeamMemberFactory WithTeamRole(TeamRole teamRole);
}
