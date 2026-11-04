using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Models.Orders;

namespace Sales.Infrastructure.Configurations;

internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        // SAL-R11: only the provider's own reference is stored — never a card number.
        builder.Property(p => p.ProviderReference).IsRequired();
        builder.HasIndex(p => p.ProviderReference).IsUnique();

        builder.Property(p => p.Result)
            .HasConversion(result => result!.Name, name => Enumeration.FromName<PaymentResult>(name))
            .HasMaxLength(20);
    }
}
