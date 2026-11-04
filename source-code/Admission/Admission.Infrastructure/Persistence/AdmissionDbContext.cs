using System.Reflection;
using Admission.Domain.Models.Performances;
using Admission.Domain.Models.Teams;
using Admission.Domain.Models.Tickets;

namespace Admission.Infrastructure.Persistence;

internal class AdmissionDbContext : BaseDbContext<AdmissionDbContext>
{
    public AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; } = default!;

    public DbSet<Performance> Performances { get; set; } = default!;

    public DbSet<TeamMember> TeamMembers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
