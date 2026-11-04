using Sales.Domain.Exceptions;
using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;

namespace Sales.Domain.Factories;

/// <summary>
/// SAL-R7: each line keeps the Ticket type's Price as it is now. SAL-R8, SAL-R9: the Booking fee, Face
/// value and Order total are worked out once, by the Order.
/// </summary>
internal class OrderFactory : IOrderFactory
{
    private readonly List<(Guid TicketTypeId, int NumberOfTickets)> lines = new();
    private Buyer? buyer;
    private Performance? performance;
    private DateTime? placedAt;

    public IOrderFactory ForBuyer(Guid accountId, string name, string email)
    {
        buyer = new Buyer(accountId, name, email);
        return this;
    }

    public IOrderFactory ForPerformance(Performance performance)
    {
        this.performance = performance;
        return this;
    }

    public IOrderFactory WithLine(Guid ticketTypeId, int numberOfTickets)
    {
        lines.Add((ticketTypeId, numberOfTickets));
        return this;
    }

    public IOrderFactory PlacedAt(DateTime utc)
    {
        placedAt = utc;
        return this;
    }

    public Order Build()
    {
        if (buyer is null || performance is null || placedAt is null)
        {
            throw new InvalidOperationException("An Order needs a buyer, a Performance and a time.");
        }

        var orderLines = lines
            .Select(l =>
            {
                var offered = performance.OfferedTicketTypes.FirstOrDefault(o => o.TicketTypeId == l.TicketTypeId)
                              ?? throw new CheckoutRefusedException("This ticket type is not part of the performance.");

                return new OrderLine(offered.TicketTypeId, offered.Name, l.NumberOfTickets, offered.Price);
            })
            .ToList();

        return new Order(
            buyer,
            performance.PerformanceId,
            performance.OrganizerId,
            performance.EventName,
            performance.StartsAt,
            placedAt.Value,
            orderLines);
    }
}
