using Accounts.Application.Organizers;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Models.Organizers;
using Accounts.Domain.Repositories;
using Accounts.Infrastructure.Persistence;

namespace Accounts.Infrastructure.Repositories;

/// <summary>One implementation for both sides: the domain repository and the query repository.</summary>
internal class OrganizerRepository : DataRepository<AccountsDbContext, Organizer>,
    IOrganizerDomainRepository,
    IOrganizerQueryRepository
{
    public OrganizerRepository(AccountsDbContext db) : base(db)
    {
    }

    public async Task<Organizer?> Find(Guid id, CancellationToken cancellationToken = default)
        => await All()
            .Include(o => o.TeamMembers)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    // ACC-V1
    public async Task<List<OrganizerListItem>> GetAll(CancellationToken cancellationToken = default)
    {
        var organizers = await AllAsNoTracking()
            .Include(o => o.TeamMembers)
            .OrderBy(o => o.OrganizerName)
            .ToListAsync(cancellationToken);

        var accounts = await AccountsFor(organizers, cancellationToken);

        return organizers
            .Select(o => new OrganizerListItem(o.Id, o.OrganizerName, Members(o, accounts)))
            .ToList();
    }

    // ACC-V2
    public async Task<List<TeamMemberResponse>> Team(Guid organizerId, CancellationToken cancellationToken = default)
    {
        var organizer = await AllAsNoTracking()
            .Include(o => o.TeamMembers)
            .FirstOrDefaultAsync(o => o.Id == organizerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Organizer), organizerId);

        return Members(organizer, await AccountsFor([organizer], cancellationToken));
    }

    public async Task<List<MyTeam>> TeamsOf(Guid accountId, CancellationToken cancellationToken = default)
    {
        var organizers = await AllAsNoTracking()
            .Include(o => o.TeamMembers)
            .Where(o => o.TeamMembers.Any(m => m.AccountId == accountId))
            .OrderBy(o => o.OrganizerName)
            .ToListAsync(cancellationToken);

        return organizers
            .Select(o => new MyTeam(o.Id, o.OrganizerName, o.TeamRoleOf(accountId)!.Name))
            .ToList();
    }

    private async Task<Dictionary<Guid, Account>> AccountsFor(
        IEnumerable<Organizer> organizers,
        CancellationToken cancellationToken)
    {
        var ids = organizers.SelectMany(o => o.TeamMembers).Select(m => m.AccountId).Distinct().ToList();

        return await Data.Set<Account>()
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);
    }

    private static List<TeamMemberResponse> Members(Organizer organizer, Dictionary<Guid, Account> accounts)
        => organizer.TeamMembers
            .Select(m => new TeamMemberResponse(
                m.AccountId,
                accounts[m.AccountId].Name,
                accounts[m.AccountId].Email,
                m.TeamRole.Name))
            .OrderBy(m => m.TeamRole)
            .ThenBy(m => m.Name)
            .ToList();
}
