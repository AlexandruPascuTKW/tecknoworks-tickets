using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Models.Orders;

namespace Sales.Infrastructure.Configurations;

internal class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.Property(l => l.TicketTypeId).IsRequired();
        builder.Property(l => l.TicketTypeName).IsRequired();
        builder.Property(l => l.NumberOfTickets).IsRequired();
        builder.Property(l => l.TicketPrice).HasConversion(Converters.Cents).HasColumnName("TicketPriceCents").IsRequired();
    }
}
