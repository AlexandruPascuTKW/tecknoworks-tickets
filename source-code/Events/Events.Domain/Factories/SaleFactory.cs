using Events.Domain.Models.Sales;

namespace Events.Domain.Factories;

internal class SaleFactory : ISaleFactory
{
    private readonly List<(Guid TicketTypeId, int NumberOfTickets)> lines = new();
    private Guid? orderId;
    private Guid? performanceId;

    public ISaleFactory ForOrder(Guid orderId, Guid performanceId)
    {
        this.orderId = orderId;
        this.performanceId = performanceId;
        return this;
    }

    public ISaleFactory WithLine(Guid ticketTypeId, int numberOfTickets)
    {
        lines.Add((ticketTypeId, numberOfTickets));
        return this;
    }

    public Sale Build()
    {
        if (orderId is null || performanceId is null)
        {
            throw new InvalidOperationException("A Sale needs an Order and a Performance.");
        }

        return new Sale(orderId.Value, performanceId.Value, lines);
    }
}
