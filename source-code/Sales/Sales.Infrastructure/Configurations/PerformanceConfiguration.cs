using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Models.Performances;

namespace Sales.Infrastructure.Configurations;

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
        builder.Property(p => p.EventName).IsRequired().HasMaxLength(PerformanceModelConstants.Performance.MaxNameLength);
        builder.Property(p => p.StartsAt).IsRequired();

        // ADR-002: two changes to the same Performance can never both win.
        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.HasMany(p => p.OfferedTicketTypes)
            .WithOne()
            .HasForeignKey("SalesPerformanceId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.OfferedTicketTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
