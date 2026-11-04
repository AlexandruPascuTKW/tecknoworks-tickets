using Admission.Domain.Models.Performances;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Configurations;

internal class PerformanceConfiguration : IEntityTypeConfiguration<Performance>
{
    public void Configure(EntityTypeBuilder<Performance> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.PerformanceId).IsRequired();
        builder.HasIndex(p => p.PerformanceId).IsUnique();

        builder.Property(p => p.EventId).IsRequired();
        builder.HasIndex(p => p.EventId);

        builder.Property(p => p.OrganizerId).IsRequired();
        builder.Property(p => p.EventName).IsRequired();
        builder.Property(p => p.VenueName).IsRequired();
        builder.Property(p => p.Date).IsRequired();
        builder.Property(p => p.DoorsTime).IsRequired();
        builder.Property(p => p.StartTime).IsRequired();
    }
}
