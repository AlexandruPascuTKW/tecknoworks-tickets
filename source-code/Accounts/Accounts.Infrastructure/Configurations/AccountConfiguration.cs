using Accounts.Domain.Models.Accounts;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.Infrastructure.Configurations;

internal class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(AccountModelConstants.Account.MaxNameLength);

        // ACC-R2: one Account per Email address — the last guard against a race.
        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(AccountModelConstants.Account.MaxEmailLength);
        builder.HasIndex(a => a.Email).IsUnique();

        builder.Property(a => a.PasswordHash);

        builder.Property(a => a.PlatformRole)
            .HasConversion(r => r!.Name, name => Enumeration.FromName<PlatformRole>(name))
            .HasMaxLength(30);

        // ACC-R3: only the hash of the invitation token is stored.
        builder.OwnsOne(a => a.Invitation, invitation =>
        {
            invitation.Property(i => i.TokenHash).IsRequired().HasColumnName("InvitationTokenHash");
            invitation.Property(i => i.ExpiresAt).HasColumnName("InvitationExpiresAt");
            invitation.HasIndex(i => i.TokenHash);
        });
    }
}
