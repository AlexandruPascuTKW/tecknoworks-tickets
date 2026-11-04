using Accounts.Domain.Models.Accounts;

namespace Accounts.Domain.Factories;

internal class AccountFactory : IAccountFactory
{
    private string? name;
    private EmailAddress? email;
    private string? passwordHash;
    private Invitation? invitation;
    private PlatformRole? platformRole;

    public IAccountFactory WithName(string name)
    {
        this.name = name;
        return this;
    }

    public IAccountFactory WithEmail(string email)
    {
        this.email = new EmailAddress(email);
        return this;
    }

    public IAccountFactory WithPasswordHash(string passwordHash)
    {
        this.passwordHash = passwordHash;
        return this;
    }

    public IAccountFactory WithInvitation(Invitation invitation)
    {
        this.invitation = invitation;
        return this;
    }

    public IAccountFactory WithPlatformRole(PlatformRole platformRole)
    {
        this.platformRole = platformRole;
        return this;
    }

    public Account Build()
    {
        if (name is null || email is null)
        {
            throw new InvalidOperationException("An Account needs a name and an email address.");
        }

        // ACC-R3: a Password, or an invitation to choose one.
        if (passwordHash is null && invitation is null)
        {
            throw new InvalidOperationException("An Account needs a password or an invitation.");
        }

        return new Account(name, email, passwordHash, invitation, platformRole);
    }
}
