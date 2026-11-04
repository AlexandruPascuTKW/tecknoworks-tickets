using Accounts.Application.Services;
using Accounts.Domain.Models.Accounts;

namespace Accounts.Application.Accounts.Commands.SignIn;

/// <summary>What signing in (or registering) returns: the token and who it is for.</summary>
public record SignInResponse(
    string Token,
    DateTime ExpiresAt,
    Guid AccountId,
    string Name,
    string Email,
    string? PlatformRole)
{
    internal static SignInResponse For(Account account, ITokenIssuer tokens)
    {
        var issued = tokens.Issue(account.Id, account.Name, account.Email, account.PlatformRole?.Name);

        return new SignInResponse(
            issued.Token,
            issued.ExpiresAt,
            account.Id,
            account.Name,
            account.Email,
            account.PlatformRole?.Name);
    }
}
