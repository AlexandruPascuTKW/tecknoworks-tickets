namespace Events.Domain.Models.Sales;

public class SaleLine : Entity
{
    internal SaleLine(Guid ticketTypeId, int numberOfTickets)
    {
        TicketTypeId = ticketTypeId;
        NumberOfTickets = numberOfTickets;
    }

    private SaleLine()
    {
    }

    public Guid TicketTypeId { get; private set; }

    public int NumberOfTickets { get; private set; }
}
