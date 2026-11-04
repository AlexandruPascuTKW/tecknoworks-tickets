using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sales.Application.Orders.Commands.ExpireLapsedOrders;
using Sales.Application.Settings;

namespace Sales.Infrastructure.Services;

/// <summary>
/// ADR-003 — marks Orders whose Hold ran out as Expired (SAL-R5). Availability does not wait for it: a lapsed
/// Hold already frees its tickets. This only keeps the stored status, and Events' view of sales, up to date.
/// </summary>
internal class ExpireLapsedOrdersJob : BackgroundService
{
    private readonly IServiceScopeFactory scopes;
    private readonly ILogger<ExpireLapsedOrdersJob> logger;
    private readonly TimeSpan interval;

    public ExpireLapsedOrdersJob(
        IServiceScopeFactory scopes,
        IOptions<SalesSettings> settings,
        ILogger<ExpireLapsedOrdersJob> logger)
    {
        this.scopes = scopes;
        this.logger = logger;
        interval = settings.Value.SweepInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IMediator>()
                    .Send(new ExpireLapsedOrdersCommand(), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Expiring lapsed orders failed; will try again.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
