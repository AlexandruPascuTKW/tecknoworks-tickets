using Sales.Domain.Models.Orders;

namespace Sales.Domain.Models.Performances;

/// <summary>A Ticket type as a Performance offers it: every Performance offers the full Quantity (EVT-R5).</summary>
public class OfferedTicketType : Entity
{
    internal OfferedTicketType(Guid ticketTypeId, string name, Money price, int quantity, bool onSale)
    {
        TicketTypeId = ticketTypeId;
        Name = name;
        Price = price;
        Quantity = quantity;
        OnSale = onSale;
    }

    private OfferedTicketType()
    {
        Name = default!;
        Price = default!;
    }

    /// <summary>The Ticket type's id as Events publishes it.</summary>
    public Guid TicketTypeId { get; private set; }

    public string Name { get; private set; }

    public Money Price { get; private set; }

    public int Quantity { get; private set; }

    public bool OnSale { get; private set; }

    internal void Replace(string name, Money price, int quantity, bool onSale)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
        OnSale = onSale;
    }
}
