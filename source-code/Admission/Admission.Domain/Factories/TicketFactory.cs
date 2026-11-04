using Admission.Domain.Models.Tickets;

namespace Admission.Domain.Factories;

/// <summary>Each <c>Build</c> draws a fresh Ticket code (ADM-R3), so one factory can issue a whole Order.</summary>
internal class TicketFactory : ITicketFactory
{
    private Guid? orderId;
    private Guid performanceId;
    private Guid organizerId;
    private Guid? buyerAccountId;
    private string? buyersName;
    private Guid? ticketTypeId;
    private string? ticketTypeName;

    public ITicketFactory ForOrder(Guid orderId, Guid performanceId, Guid organizerId)
    {
        this.orderId = orderId;
        this.performanceId = performanceId;
        this.organizerId = organizerId;
        return this;
    }

    public ITicketFactory ForBuyer(Guid accountId, string buyersName)
    {
        buyerAccountId = accountId;
        this.buyersName = buyersName;
        return this;
    }

    public ITicketFactory OfType(Guid ticketTypeId, string ticketTypeName)
    {
        this.ticketTypeId = ticketTypeId;
        this.ticketTypeName = ticketTypeName;
        return this;
    }

    public Ticket Build()
    {
        if (orderId is null || buyerAccountId is null || buyersName is null
            || ticketTypeId is null || ticketTypeName is null)
        {
            throw new InvalidOperationException("A Ticket needs an Order, a Buyer and a Ticket type.");
        }

        return new Ticket(
            TicketCode.New(),
            orderId.Value,
            performanceId,
            organizerId,
            ticketTypeId.Value,
            ticketTypeName,
            buyerAccountId.Value,
            buyersName);
    }
}
