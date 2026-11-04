using Events.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Web;

public static class WebConfiguration
{
    public static IServiceCollection AddEventsWebComponents(this IServiceCollection services)
        => services.AddWebComponents(typeof(EventsApplicationConfiguration));
}
