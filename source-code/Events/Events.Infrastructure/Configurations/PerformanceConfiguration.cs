using Events.Domain.Models.Events;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class PerformanceConfiguration : IEntityTypeConfiguration<Performance>
{
    public void Configure(EntityTypeBuilder<Performance> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        // ADR-006: local date and time at the Venue, stored as DateOnly / TimeOnly.
        builder.OwnsOne(p => p.Schedule, schedule =>
        {
            schedule.Property(s => s.Date).HasColumnName("Date");
            schedule.Property(s => s.DoorsTime).HasColumnName("DoorsTime");
            schedule.Property(s => s.StartTime).HasColumnName("StartTime");
        });
        builder.Navigation(p => p.Schedule).IsRequired();
    }
}
