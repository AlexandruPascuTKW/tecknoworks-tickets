using Accounts.Domain.Models.Accounts;

namespace Accounts.Domain.Repositories;

public interface IAccountDomainRepository : IDomainRepository<Account>
{
    Task<Account?> Find(Guid id, CancellationToken cancellationToken = default);

    Task<Account?> FindByEmail(EmailAddress email, CancellationToken cancellationToken = default);

    Task<Account?> FindByInvitationTokenHash(string tokenHash, CancellationToken cancellationToken = default);
}
