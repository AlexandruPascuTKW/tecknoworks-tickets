using System.Reflection;
using Events.Application.Common;
using Events.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class EventsApplicationConfiguration
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    public static IServiceCollection AddEventsApplication(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddCommonApplication(configuration, Assembly)
            .Configure<EventsSettings>(configuration.GetSection("Events"))
            .AddTransient<EventAccess>()
            .AddTransient<VenueTime>();
}
