using Events.Domain.Models.Images;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class EventImageConfiguration : IEntityTypeConfiguration<EventImage>
{
    public void Configure(EntityTypeBuilder<EventImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.ContentType).IsRequired().HasMaxLength(30);
        builder.Property(i => i.Content).IsRequired();
    }
}
