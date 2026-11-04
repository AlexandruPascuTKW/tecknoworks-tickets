using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Domain;

public static class DomainConfiguration
{
    public static IServiceCollection AddEventsDomain(this IServiceCollection services)
        => services.AddCommonDomain(Assembly.GetExecutingAssembly());
}
