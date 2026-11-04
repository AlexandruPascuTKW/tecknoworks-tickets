using Events.Domain.Models.Sales;

namespace Events.Domain.Factories;

public interface ISaleFactory : IFactory<Sale>
{
    ISaleFactory ForOrder(Guid orderId, Guid performanceId);
    ISaleFactory WithLine(Guid ticketTypeId, int numberOfTickets);
}
