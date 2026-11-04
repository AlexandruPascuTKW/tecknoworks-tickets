using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Services;
using Sales.Infrastructure.Payments;
using Sales.Infrastructure.Persistence;
using Sales.Infrastructure.Services;

namespace Sales.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddSalesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDBStorage<SalesDbContext>(configuration, Assembly.GetExecutingAssembly())
            .AddTransient<IDbInitializer, SalesDbInitializer>()
            // ADR-004: one simulated provider instance, seen as the provider and as its own hosted page.
            .AddSingleton<SimulatedPaymentProvider>()
            .AddSingleton<IPaymentProvider>(sp => sp.GetRequiredService<SimulatedPaymentProvider>())
            .AddSingleton<ISimulatedPaymentPages>(sp => sp.GetRequiredService<SimulatedPaymentProvider>())
            // ADR-003
            .AddHostedService<ExpireLapsedOrdersJob>();
}
