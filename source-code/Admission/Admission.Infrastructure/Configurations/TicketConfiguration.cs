using Admission.Domain.Models.Tickets;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Configurations;

internal class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        // ADM-R3: no two Tickets share a code — the database is the final guard.
        builder.Property(t => t.Code)
            .HasConversion(code => code.Value, value => new TicketCode(value))
            .HasColumnName("TicketCode")
            .IsRequired()
            .HasMaxLength(TicketModelConstants.Ticket.CodeLength);
        builder.HasIndex(t => t.Code).IsUnique();

        builder.Property(t => t.OrderId).IsRequired();
        builder.HasIndex(t => t.OrderId);

        builder.Property(t => t.PerformanceId).IsRequired();
        builder.HasIndex(t => t.PerformanceId);

        builder.Property(t => t.OrganizerId).IsRequired();
        builder.Property(t => t.TicketTypeId).IsRequired();
        builder.Property(t => t.TicketTypeName).IsRequired();

        builder.Property(t => t.BuyerAccountId).IsRequired();
        builder.HasIndex(t => t.BuyerAccountId);

        builder.Property(t => t.BuyersName).IsRequired();
    }
}
