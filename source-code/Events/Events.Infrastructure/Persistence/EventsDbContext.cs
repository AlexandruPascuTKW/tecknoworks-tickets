using System.Reflection;
using Events.Domain.Models.Events;
using Events.Domain.Models.Images;
using Events.Domain.Models.Sales;
using Events.Domain.Models.Teams;
using Events.Domain.Models.Venues;

namespace Events.Infrastructure.Persistence;

internal class EventsDbContext : BaseDbContext<EventsDbContext>
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
    }

    public DbSet<Venue> Venues { get; set; } = default!;

    public DbSet<Event> Events { get; set; } = default!;

    public DbSet<EventImage> EventImages { get; set; } = default!;

    public DbSet<Sale> Sales { get; set; } = default!;

    public DbSet<TeamMember> TeamMembers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
