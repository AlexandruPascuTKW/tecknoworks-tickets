using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.Domain;

public static class DomainConfiguration
{
    public static IServiceCollection AddAccountsDomain(this IServiceCollection services)
        => services.AddCommonDomain(Assembly.GetExecutingAssembly());
}
