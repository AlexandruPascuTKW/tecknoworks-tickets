using Accounts.Domain.Exceptions;

namespace Accounts.Domain.Models.Accounts;

/// <summary>A person's login (ACC-R2). The same Account buys tickets and works on Teams.</summary>
public class Account : Entity, IAggregateRoot
{
    internal Account(
        string name,
        EmailAddress email,
        string? passwordHash,
        Invitation? invitation,
        PlatformRole? platformRole)
    {
        Guard.ForStringLength(name, 1, AccountModelConstants.Account.MaxNameLength, nameof(name));

        Name = name.Trim();
        Email = email.Value;
        PasswordHash = passwordHash;
        Invitation = invitation;
        PlatformRole = platformRole;
    }

    // EF materialization.
    private Account()
    {
        Name = default!;
        Email = default!;
    }

    public string Name { get; private set; }

    /// <summary>ACC-F2: normalised lower-case; unique.</summary>
    public string Email { get; private set; }

    public string? PasswordHash { get; private set; }

    public PlatformRole? PlatformRole { get; private set; }

    /// <summary>Set while an Account made for someone else waits for its owner to choose a Password (ACC-R3).</summary>
    public Invitation? Invitation { get; private set; }

    public bool HasPassword => PasswordHash is not null;

    /// <summary>ACC-R3: the person chooses their own Password from the emailed link.</summary>
    public Account AcceptInvitation(string passwordHash, DateTime now)
    {
        if (Invitation is null)
        {
            throw new InvalidAccountException("This invitation has already been used.");
        }

        if (Invitation.HasExpired(now))
        {
            throw new InvalidAccountException("This invitation has expired.");
        }

        PasswordHash = passwordHash;
        Invitation = null;
        return this;
    }
}
