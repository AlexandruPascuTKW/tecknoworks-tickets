using Accounts.Domain.Models.Organizers;

namespace Accounts.Domain.Factories;

public interface IOrganizerFactory : IFactory<Organizer>
{
    IOrganizerFactory WithName(string organizerName);
    IOrganizerFactory WithFirstOwner(Guid accountId);
}
