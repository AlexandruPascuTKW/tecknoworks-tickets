using System.Reflection;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Models.Organizers;

namespace Accounts.Infrastructure.Persistence;

internal class AccountsDbContext : BaseDbContext<AccountsDbContext>
{
    public AccountsDbContext(DbContextOptions<AccountsDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = default!;

    public DbSet<Organizer> Organizers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
