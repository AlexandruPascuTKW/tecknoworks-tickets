using System.Security.Cryptography;
using System.Text;

namespace Accounts.Domain.Models.Accounts;

/// <summary>
/// ACC-R3 / ADR-007 — a single-use invitation to choose a Password. Only the hash of the token is kept.
/// </summary>
public class Invitation : ValueObject
{
    private Invitation()
    {
        TokenHash = default!;
    }

    public string TokenHash { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <summary>Issues a new invitation; returns it with the plain token to put in the link.</summary>
    public static (Invitation Invitation, string Token) Issue(DateTime now, TimeSpan lifetime)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        return (new Invitation { TokenHash = HashOf(token), ExpiresAt = now + lifetime }, token);
    }

    public static string HashOf(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public bool HasExpired(DateTime now) => now >= ExpiresAt;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TokenHash;
        yield return ExpiresAt;
    }
}
