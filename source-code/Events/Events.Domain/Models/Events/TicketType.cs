using Events.Domain.Exceptions;

namespace Events.Domain.Models.Events;

/// <summary>
/// A kind of ticket the Event sells (EVT-F12 to EVT-F15). Set up once for the whole Event: every
/// Performance offers its full Quantity (EVT-R5).
/// </summary>
public class TicketType : Entity
{
    internal TicketType(string name, Price price, int quantity)
    {
        Name = ValidName(name);
        Price = price;
        Quantity = ValidQuantity(quantity);
        OnSale = true; // EVT-R10: On sale when it is added.
    }

    private TicketType()
    {
        Name = default!;
        Price = default!;
    }

    public string Name { get; private set; }

    public Price Price { get; private set; }

    public int Quantity { get; private set; }

    public bool OnSale { get; private set; }

    internal void Rename(string name) => Name = ValidName(name);

    internal void Reprice(Price price) => Price = price;

    internal void Requantity(int quantity) => Quantity = ValidQuantity(quantity);

    internal void TakeOffSale() => OnSale = false;

    private static string ValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > EventModelConstants.TicketType.MaxNameLength)
        {
            throw new InvalidEventException("Ticket type name is required.");
        }

        return name.Trim();
    }

    // EVT-F14: a whole number above zero.
    private static int ValidQuantity(int quantity)
        => quantity > 0
            ? quantity
            : throw new InvalidEventException("Quantity must be a whole number above zero.");
}
