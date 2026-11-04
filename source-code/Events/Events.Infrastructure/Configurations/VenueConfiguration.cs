using Events.Domain.Models.Venues;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.OrganizerId).IsRequired();
        builder.HasIndex(v => v.OrganizerId);

        builder.Property(v => v.VenueName).IsRequired().HasMaxLength(VenueModelConstants.Venue.MaxNameLength);
        builder.Property(v => v.Address).IsRequired().HasMaxLength(VenueModelConstants.Venue.MaxAddressLength);
        builder.Property(v => v.Capacity).IsRequired();
    }
}
