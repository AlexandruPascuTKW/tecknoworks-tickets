using Accounts.Application.Services;
using Accounts.Application.Settings;
using Accounts.Domain.Factories;
using Accounts.Domain.Models.Accounts;
using Microsoft.Extensions.Options;

namespace Accounts.Infrastructure.Persistence;

/// <summary>
/// Migrates, then seeds the TecknoworksTickets staff (Platform admin, Finance manager) from configuration.
/// No use case in the ACC spec creates them.
/// </summary>
internal class AccountsDbInitializer : DbInitializer
{
    private readonly AccountsDbContext db;
    private readonly IAccountFactory factory;
    private readonly IPasswordHasher hasher;
    private readonly AccountsSettings settings;

    public AccountsDbInitializer(
        AccountsDbContext db,
        IAccountFactory factory,
        IPasswordHasher hasher,
        IOptions<AccountsSettings> settings)
        : base(db)
    {
        this.db = db;
        this.factory = factory;
        this.hasher = hasher;
        this.settings = settings.Value;
    }

    public override void Initialize()
    {
        base.Initialize();

        foreach (var staff in settings.SeededStaff)
        {
            var email = new EmailAddress(staff.Email).Value;

            if (db.Accounts.Any(a => a.Email == email))
            {
                continue;
            }

            db.Accounts.Add(factory
                .WithName(staff.Name)
                .WithEmail(email)
                .WithPasswordHash(hasher.Hash(staff.Password))
                .WithPlatformRole(Enumeration.FromName<PlatformRole>(staff.PlatformRole))
                .Build());
        }

        db.SaveChanges();
    }
}
