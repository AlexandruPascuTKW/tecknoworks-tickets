using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Models.Orders;

namespace Sales.Infrastructure.Configurations;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.OwnsOne(o => o.Buyer, buyer =>
        {
            buyer.Property(b => b.AccountId).HasColumnName("BuyerAccountId");
            buyer.Property(b => b.Name).HasColumnName("BuyerName").IsRequired();
            buyer.Property(b => b.Email).HasColumnName("BuyerEmail").IsRequired();
            buyer.HasIndex(b => b.AccountId); // SAL-R15: find an Attendee's Orders for a Performance
        });
        builder.Navigation(o => o.Buyer).IsRequired();

        builder.Property(o => o.PerformanceId).IsRequired();
        builder.Property(o => o.OrganizerId).IsRequired();
        builder.Property(o => o.EventName).IsRequired();
        builder.Property(o => o.PerformanceStartsAt).IsRequired();

        builder.Property(o => o.Status)
            .HasConversion(status => status.Name, name => Enumeration.FromName<OrderStatus>(name))
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.PlacedAt).HasConversion(Converters.Utc).IsRequired();
        builder.Property(o => o.HoldEndsAt).HasConversion(Converters.Utc).IsRequired();

        builder.Property(o => o.FaceValue).HasConversion(Converters.Cents).HasColumnName("FaceValueCents");
        builder.Property(o => o.BookingFee).HasConversion(Converters.Cents).HasColumnName("BookingFeeCents");
        builder.Property(o => o.OrderTotal).HasConversion(Converters.Cents).HasColumnName("OrderTotalCents");

        // SAL-R6: Availability is worked out from a Performance's Orders by status and Hold.
        builder.HasIndex(o => new { o.PerformanceId, o.Status, o.HoldEndsAt });

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.Payments)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
