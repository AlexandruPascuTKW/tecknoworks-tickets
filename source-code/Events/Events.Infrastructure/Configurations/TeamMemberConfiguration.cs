using Events.Domain.Models.Teams;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.OrganizerId).IsRequired();
        builder.Property(m => m.AccountId).IsRequired();
        builder.HasIndex(m => new { m.OrganizerId, m.AccountId }).IsUnique();

        builder.Property(m => m.TeamRole)
            .HasConversion(role => role.Name, name => Enumeration.FromName<TeamRole>(name))
            .IsRequired()
            .HasMaxLength(20);
    }
}
