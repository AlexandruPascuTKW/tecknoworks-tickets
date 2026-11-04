using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Common;
using Sales.Application.Handlers;
using Sales.Application.Settings;

namespace Sales.Application;

public static class SalesApplicationConfiguration
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    public static IServiceCollection AddSalesApplication(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddCommonApplication(configuration, Assembly)
            .Configure<SalesSettings>(configuration.GetSection("Sales"))
            .AddTransient<SalesClock>()
            .AddTransient<PerformanceSynchronizer>();
}
