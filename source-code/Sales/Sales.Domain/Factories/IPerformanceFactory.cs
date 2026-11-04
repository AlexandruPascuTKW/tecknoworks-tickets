using Sales.Domain.Models.Performances;

namespace Sales.Domain.Factories;

public interface IPerformanceFactory : IFactory<Performance>
{
    IPerformanceFactory ForPerformance(Guid performanceId, Guid eventId, Guid organizerId, string eventName, DateTime startsAt);
    IPerformanceFactory WithTicketType(OfferedTicketTypeData ticketType);
}
