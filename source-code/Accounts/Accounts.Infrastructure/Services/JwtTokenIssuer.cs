using System.Security.Claims;
using System.Text;
using Accounts.Application.Services;
using Accounts.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Accounts.Infrastructure.Services;

/// <summary>
/// ADR-001: signs with the same key <c>AddTokenAuthentication</c> validates with. The token carries the
/// Account id, name, email and platform role — never a Team role.
/// </summary>
internal class JwtTokenIssuer : ITokenIssuer
{
    private readonly ApplicationSettings application;
    private readonly AccountsSettings accounts;
    private readonly TimeProvider clock;

    public JwtTokenIssuer(
        IOptions<ApplicationSettings> application,
        IOptions<AccountsSettings> accounts,
        TimeProvider clock)
    {
        this.application = application.Value;
        this.accounts = accounts.Value;
        this.clock = clock;
    }

    public IssuedToken Issue(Guid accountId, string name, string email, string? platformRole)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
        };

        if (platformRole is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, platformRole));
        }

        var expires = clock.GetUtcNow().UtcDateTime + accounts.TokenLifetime;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(application.Secret)),
                SecurityAlgorithms.HmacSha256),
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
