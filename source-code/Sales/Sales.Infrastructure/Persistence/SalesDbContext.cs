using System.Reflection;
using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;

namespace Sales.Infrastructure.Persistence;

internal class SalesDbContext : BaseDbContext<SalesDbContext>
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }

    public DbSet<Performance> Performances { get; set; } = default!;

    public DbSet<Order> Orders { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
