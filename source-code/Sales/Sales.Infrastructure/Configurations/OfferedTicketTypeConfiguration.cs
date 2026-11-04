using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Models.Performances;

namespace Sales.Infrastructure.Configurations;

internal class OfferedTicketTypeConfiguration : IEntityTypeConfiguration<OfferedTicketType>
{
    public void Configure(EntityTypeBuilder<OfferedTicketType> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.TicketTypeId).IsRequired();
        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Price).HasConversion(Converters.Cents).HasColumnName("PriceCents").IsRequired();
        builder.Property(t => t.Quantity).IsRequired();
        builder.Property(t => t.OnSale).IsRequired();
    }
}
