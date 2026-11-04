using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Delivers one context's outbox rows, oldest first, through <see cref="IEventDispatcher"/> (ADR-005).
/// Delivery is at least once: handlers must be idempotent. A row that keeps failing is abandoned after
/// <see cref="MaxAttempts"/> so it cannot block the ones behind it.
/// </summary>
internal sealed class OutboxDispatcher<TDbContext> : BackgroundService where TDbContext : DbContext
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory scopes;
    private readonly ILogger<OutboxDispatcher<TDbContext>> logger;

    public OutboxDispatcher(IServiceScopeFactory scopes, ILogger<OutboxDispatcher<TDbContext>> logger)
    {
        this.scopes = scopes;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeliverPending(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Outbox delivery for {Context} failed; will retry.", typeof(TDbContext).Name);
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task DeliverPending(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

        var pending = await db.Set<OutboxMessage>()
            .Where(m => m.DeliveredAt == null)
            .OrderBy(m => m.Id)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                var type = Type.GetType(message.Type, throwOnError: true)!;
                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(message.Payload, type)!;

                await dispatcher.Dispatch(domainEvent);

                message.DeliveredAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.Attempts++;
                message.Error = ex.Message;

                if (message.Attempts >= MaxAttempts)
                {
                    logger.LogError(ex, "Abandoning outbox message {Id} ({Type}).", message.Id, message.Type);
                    message.DeliveredAt = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(cancellationToken);
                break; // keep order: try this one again before the ones behind it
            }
        }
    }
}
