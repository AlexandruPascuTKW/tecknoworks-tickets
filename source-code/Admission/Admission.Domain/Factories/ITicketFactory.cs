using Admission.Domain.Models.Tickets;

namespace Admission.Domain.Factories;

/// <summary>ADM-R2 — one Ticket for every ticket on a Confirmed Order, each with a code of its own.</summary>
public interface ITicketFactory : IFactory<Ticket>
{
    ITicketFactory ForOrder(Guid orderId, Guid performanceId, Guid organizerId);
    ITicketFactory ForBuyer(Guid accountId, string buyersName);
    ITicketFactory OfType(Guid ticketTypeId, string ticketTypeName);
}
