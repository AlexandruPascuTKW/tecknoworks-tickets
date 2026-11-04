using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;

namespace Sales.Domain.Factories;

public interface IOrderFactory : IFactory<Order>
{
    IOrderFactory ForBuyer(Guid accountId, string name, string email);
    IOrderFactory ForPerformance(Performance performance);
    IOrderFactory WithLine(Guid ticketTypeId, int numberOfTickets);
    IOrderFactory PlacedAt(DateTime utc);
}
