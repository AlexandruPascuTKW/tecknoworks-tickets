using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Sales.Domain;

public static class DomainConfiguration
{
    public static IServiceCollection AddSalesDomain(this IServiceCollection services)
        => services.AddCommonDomain(Assembly.GetExecutingAssembly());
}
