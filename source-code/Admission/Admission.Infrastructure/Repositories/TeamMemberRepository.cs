using Admission.Domain.Models.Teams;
using Admission.Domain.Repositories;
using Admission.Infrastructure.Persistence;

namespace Admission.Infrastructure.Repositories;

internal class TeamMemberRepository : DataRepository<AdmissionDbContext, TeamMember>, ITeamMemberDomainRepository
{
    public TeamMemberRepository(AdmissionDbContext db) : base(db)
    {
    }

    public async Task<TeamMember?> Find(Guid organizerId, Guid accountId, CancellationToken cancellationToken = default)
        => await All().FirstOrDefaultAsync(
            m => m.OrganizerId == organizerId && m.AccountId == accountId,
            cancellationToken);

    public async Task Delete(TeamMember member, CancellationToken cancellationToken = default)
    {
        Data.Remove(member);
        await Data.SaveChangesAsync(cancellationToken);
    }
}
