namespace Admission.Domain.Models.Tickets;

/// <summary>
/// One person's way in to one Performance, issued from a Confirmed Order (ADM-R2). The Buyer's name, the
/// Ticket type's name and the Organizer are kept as they were when the Order was confirmed, so nothing here
/// needs to ask Sales or Accounts.
/// </summary>
public class Ticket : Entity, IAggregateRoot
{
    internal Ticket(
        TicketCode code,
        Guid orderId,
        Guid performanceId,
        Guid organizerId,
        Guid ticketTypeId,
        string ticketTypeName,
        Guid buyerAccountId,
        string buyersName)
    {
        Code = code;
        OrderId = orderId;
        PerformanceId = performanceId;
        OrganizerId = organizerId;
        TicketTypeId = ticketTypeId;
        TicketTypeName = ticketTypeName;
        BuyerAccountId = buyerAccountId;
        BuyersName = buyersName;
    }

    private Ticket()
    {
        Code = default!;
        TicketTypeName = default!;
        BuyersName = default!;
    }

    /// <summary>ADM-F1 / ADM-R3: no two Tickets share a code.</summary>
    public TicketCode Code { get; private set; }

    /// <summary>ADM-F2: the Confirmed Order it was issued from — Sales' id, a plain Guid.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>ADM-F3: as Events publishes it.</summary>
    public Guid PerformanceId { get; private set; }

    public Guid OrganizerId { get; private set; }

    public Guid TicketTypeId { get; private set; }

    /// <summary>ADM-F4.</summary>
    public string TicketTypeName { get; private set; }

    /// <summary>The Buyer — the Attendee who placed the Order (ADM-R1: only they see this Ticket).</summary>
    public Guid BuyerAccountId { get; private set; }

    /// <summary>ADM-F5.</summary>
    public string BuyersName { get; private set; }
}
