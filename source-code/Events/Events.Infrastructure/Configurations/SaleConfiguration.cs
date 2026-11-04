using Events.Domain.Models.Sales;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.OrderId).IsRequired();
        builder.HasIndex(s => s.OrderId).IsUnique(); // one Sale per Sales Order: handlers can be replayed

        builder.Property(s => s.PerformanceId).IsRequired();
        builder.HasIndex(s => new { s.PerformanceId, s.HasSales });

        builder.HasMany(s => s.Lines)
            .WithOne()
            .HasForeignKey("SaleId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
