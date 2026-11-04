namespace Sales.Domain.Models.Orders;

/// <summary>One Ticket type on an Order (SAL glossary). Keeps the Price as it was when placed (SAL-R7).</summary>
public class OrderLine : Entity
{
    internal OrderLine(Guid ticketTypeId, string ticketTypeName, int numberOfTickets, Money ticketPrice)
    {
        TicketTypeId = ticketTypeId;
        TicketTypeName = ticketTypeName;
        NumberOfTickets = numberOfTickets;
        TicketPrice = ticketPrice;
    }

    private OrderLine()
    {
        TicketTypeName = default!;
        TicketPrice = default!;
    }

    public Guid TicketTypeId { get; private set; }

    public string TicketTypeName { get; private set; }

    /// <summary>SAL-F10: a whole number from 1 to 8.</summary>
    public int NumberOfTickets { get; private set; }

    /// <summary>SAL-F11: the Price of one ticket when the Order was placed.</summary>
    public Money TicketPrice { get; private set; }
}
