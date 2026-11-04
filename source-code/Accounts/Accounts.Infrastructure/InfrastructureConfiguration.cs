using System.Reflection;
using Accounts.Application.Services;
using Accounts.Infrastructure.Persistence;
using Accounts.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddAccountsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDBStorage<AccountsDbContext>(configuration, Assembly.GetExecutingAssembly())
            .AddTransient<IDbInitializer, AccountsDbInitializer>()
            .AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>()
            .AddSingleton<ITokenIssuer, JwtTokenIssuer>()
            .AddSingleton<IInvitationSender, LoggingInvitationSender>();
}
