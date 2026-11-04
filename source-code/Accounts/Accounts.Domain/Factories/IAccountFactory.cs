using Accounts.Domain.Models.Accounts;

namespace Accounts.Domain.Factories;

public interface IAccountFactory : IFactory<Account>
{
    IAccountFactory WithName(string name);
    IAccountFactory WithEmail(string email);
    IAccountFactory WithPasswordHash(string passwordHash);
    IAccountFactory WithInvitation(Invitation invitation);
    IAccountFactory WithPlatformRole(PlatformRole platformRole);
}
