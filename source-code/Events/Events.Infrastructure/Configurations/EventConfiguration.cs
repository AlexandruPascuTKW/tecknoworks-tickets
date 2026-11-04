using Events.Domain.Models.Events;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.OrganizerId).IsRequired();
        builder.HasIndex(e => e.OrganizerId);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(EventModelConstants.Event.MaxNameLength);
        builder.Property(e => e.Description).HasMaxLength(EventModelConstants.Event.MaxDescriptionLength);
        builder.Property(e => e.ImageId);

        builder.Property(e => e.Status)
            .HasConversion(s => s.Name, name => Enumeration.FromName<EventStatus>(name))
            .IsRequired()
            .HasMaxLength(20);

        // The Venue's details, copied onto the Event (EVT-R6 is checked inside the Event alone).
        builder.OwnsOne(e => e.Venue, venue =>
        {
            venue.Property(v => v.VenueId).HasColumnName("VenueId");
            venue.Property(v => v.VenueName).HasColumnName("VenueName").IsRequired();
            venue.Property(v => v.Address).HasColumnName("VenueAddress").IsRequired();
            venue.Property(v => v.Capacity).HasColumnName("VenueCapacity");
        });
        builder.Navigation(e => e.Venue).IsRequired();

        builder.HasMany(e => e.Performances)
            .WithOne()
            .HasForeignKey("EventId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.Performances).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.TicketTypes)
            .WithOne()
            .HasForeignKey("EventId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.TicketTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
