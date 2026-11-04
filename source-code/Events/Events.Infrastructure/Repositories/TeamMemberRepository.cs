using Events.Domain.Models.Teams;
using Events.Domain.Repositories;
using Events.Infrastructure.Persistence;

namespace Events.Infrastructure.Repositories;

internal class TeamMemberRepository : DataRepository<EventsDbContext, TeamMember>, ITeamMemberDomainRepository
{
    public TeamMemberRepository(EventsDbContext db) : base(db)
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
