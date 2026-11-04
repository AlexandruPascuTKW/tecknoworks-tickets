using Accounts.Domain.Models.Organizers;

namespace Accounts.Domain.Repositories;

public interface IOrganizerDomainRepository : IDomainRepository<Organizer>
{
    Task<Organizer?> Find(Guid id, CancellationToken cancellationToken = default);
}
