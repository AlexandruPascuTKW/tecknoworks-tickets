using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddDBStorage<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
        where TDbContext : DbContext
        => services
            .AddDatabase<TDbContext>(configuration)
            .AddRepositories(assembly)
            // ADR-005: one outbox dispatcher per context.
            .AddHostedService<OutboxDispatcher<TDbContext>>();

    public static IServiceCollection AddTokenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secret = configuration
            .GetSection(nameof(ApplicationSettings))
            .GetValue<string>(nameof(ApplicationSettings.Secret));

        var key = Encoding.ASCII.GetBytes(secret);

        services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddEventSourcing(this IServiceCollection services)
        => services.AddTransient<IEventDispatcher, EventDispatcher>();

    public static IHttpClientBuilder ConfigureDefaultHttpClientHandler(this IHttpClientBuilder builder)
        => builder
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

    private static IServiceCollection AddDatabase<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext
        => services
            .AddScoped<DbContext, TDbContext>()
            .AddDbContext<TDbContext>(options => options
                .UseSqlite(
                    configuration.GetDefaultConnectionString(),
                    sqlite => sqlite
                        .MigrationsAssembly(typeof(TDbContext).Assembly.FullName)
                        // Every context shares one database, so each keeps its own history table.
                        .MigrationsHistoryTable($"__EFMigrationsHistory_{typeof(TDbContext).Name}")));

    internal static IServiceCollection AddRepositories(
        this IServiceCollection services,
        Assembly assembly)
        => services
            .Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .AssignableToAny(typeof(IDomainRepository<>), typeof(IQueryRepository<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime());
}