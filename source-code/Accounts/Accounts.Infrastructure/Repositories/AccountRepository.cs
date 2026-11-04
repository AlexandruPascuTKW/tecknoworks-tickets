using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Repositories;
using Accounts.Infrastructure.Persistence;

namespace Accounts.Infrastructure.Repositories;

internal class AccountRepository : DataRepository<AccountsDbContext, Account>, IAccountDomainRepository
{
    public AccountRepository(AccountsDbContext db) : base(db)
    {
    }

    public async Task<Account?> Find(Guid id, CancellationToken cancellationToken = default)
        => await All().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Account?> FindByEmail(EmailAddress email, CancellationToken cancellationToken = default)
    {
        var value = email.Value;
        return await All().FirstOrDefaultAsync(a => a.Email == value, cancellationToken);
    }

    public async Task<Account?> FindByInvitationTokenHash(string tokenHash, CancellationToken cancellationToken = default)
        => await All().FirstOrDefaultAsync(
            a => a.Invitation != null && a.Invitation.TokenHash == tokenHash,
            cancellationToken);
}
