using Accounts.Domain.Models.Organizers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.Infrastructure.Configurations;

internal class OrganizerConfiguration : IEntityTypeConfiguration<Organizer>
{
    public void Configure(EntityTypeBuilder<Organizer> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.OrganizerName)
            .IsRequired()
            .HasMaxLength(OrganizerModelConstants.Organizer.MaxNameLength);

        builder.HasMany(o => o.TeamMembers)
            .WithOne()
            .HasForeignKey("OrganizerId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.TeamMembers).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
