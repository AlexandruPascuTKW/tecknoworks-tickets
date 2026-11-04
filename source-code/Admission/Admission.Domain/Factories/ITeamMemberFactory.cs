using Admission.Domain.Models.Teams;

namespace Admission.Domain.Factories;

public interface ITeamMemberFactory : IFactory<TeamMember>
{
    ITeamMemberFactory OnTeamOf(Guid organizerId);
    ITeamMemberFactory ForAccount(Guid accountId);
    ITeamMemberFactory WithTeamRole(TeamRole teamRole);
}
