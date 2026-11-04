using System.Reflection;
using Events.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddEventsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDBStorage<EventsDbContext>(configuration, Assembly.GetExecutingAssembly())
            .AddTransient<IDbInitializer, EventsDbInitializer>();
}
