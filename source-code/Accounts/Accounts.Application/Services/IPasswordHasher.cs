namespace Accounts.Application.Services;

/// <summary>ADR-001: password storage. Implemented in Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
