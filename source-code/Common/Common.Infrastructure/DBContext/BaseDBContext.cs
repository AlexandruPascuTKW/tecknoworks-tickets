using System.Text.Json;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Base of every context's DbContext. Events raised by aggregates are written to this context's outbox
/// table in the same save as the aggregate, and delivered afterwards by <c>OutboxDispatcher</c> (ADR-005).
/// Subclasses must call <c>base.OnModelCreating</c>.
/// </summary>
public abstract class BaseDbContext<TContext> : DbContext where TContext : DbContext
{
    protected BaseDbContext(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            // Every context shares one database, so each keeps its own outbox table.
            outbox.ToTable($"__Outbox_{typeof(TContext).Name}");
            outbox.HasKey(m => m.Id);
            outbox.Property(m => m.Id).ValueGeneratedOnAdd();
            outbox.Property(m => m.Type).IsRequired();
            outbox.Property(m => m.Payload).IsRequired();
            outbox.HasIndex(m => m.DeliveredAt);
        });

        // Every context shares one database, so each context's tables carry its name: "Sales_Orders".
        // Without it, two contexts with a Performances or TeamMembers table would collide.
        var prefix = typeof(TContext).Name.Replace("DbContext", string.Empty);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => !e.IsOwned() && e.ClrType != typeof(OutboxMessage)))
        {
            var table = entityType.GetTableName();

            if (table is not null)
            {
                entityType.SetTableName($"{prefix}_{table}");
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // New entities get their Ids here, so deferred events can mention them.
        ChangeTracker.DetectChanges();

        var entitiesWithEvents = ChangeTracker
            .Entries<IEntity>()
            .Where(e => e.Entity.Events.Any())
            .Select(e => e.Entity)
            .ToArray();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var raised in entity.Events)
            {
                var domainEvent = raised is DeferredDomainEvent deferred ? deferred.Resolve() : raised;
                var type = domainEvent.GetType();

                Set<OutboxMessage>().Add(new OutboxMessage(
                    type.AssemblyQualifiedName!,
                    JsonSerializer.Serialize(domainEvent, type),
                    DateTime.UtcNow));
            }

            entity.ClearEvents();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
