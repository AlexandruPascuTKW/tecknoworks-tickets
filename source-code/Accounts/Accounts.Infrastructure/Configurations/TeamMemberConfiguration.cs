using Accounts.Domain.Models.Organizers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.Infrastructure.Configurations;

internal class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.AccountId).IsRequired();

        builder.Property(m => m.TeamRole)
            .HasConversion(r => r.Name, name => Enumeration.FromName<TeamRole>(name))
            .HasMaxLength(20);

        // The foreign key to the Organizer is declared in OrganizerConfiguration; declaring the shadow
        // property here too means the index below does not depend on configuration order.
        builder.Property<Guid>("OrganizerId");

        // ACC-R5: nobody is on a Team twice.
        builder.HasIndex("OrganizerId", nameof(TeamMember.AccountId)).IsUnique();
    }
}
