namespace Accounts.Application.Services;

public record IssuedToken(string Token, DateTime ExpiresAt);

/// <summary>ADR-001: the sign-in token. Carries the Account and its platform role, never Team roles.</summary>
public interface ITokenIssuer
{
    IssuedToken Issue(Guid accountId, string name, string email, string? platformRole);
}
