using Events.Domain.Models.Events;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Configurations;

internal class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(EventModelConstants.TicketType.MaxNameLength);

        // ADR-006: euros are stored as whole cents so SQLite can sum and compare them.
        builder.Property(t => t.Price)
            .HasConversion(price => decimal.ToInt64(price.Amount * 100m), cents => new Price(cents / 100m))
            .HasColumnName("PriceCents")
            .IsRequired();

        builder.Property(t => t.Quantity).IsRequired();
        builder.Property(t => t.OnSale).IsRequired();
    }
}
