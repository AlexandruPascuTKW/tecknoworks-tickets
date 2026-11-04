using Sales.Domain.Models.Performances;

namespace Sales.Application.Performances;

public record TicketTypeAvailability(Guid TicketTypeId, string TicketTypeName, bool SoldOut);

/// <summary>SAL-V2 — whether a Performance and its On sale Ticket types are Sold out. Never a count (SAL-R14).</summary>
public record PerformanceAvailabilityResponse(
    Guid PerformanceId,
    DateTime StartsAt,
    bool SoldOut,
    List<TicketTypeAvailability> TicketTypes);

/// <summary>A Performance the Finance manager can pick to see its sales.</summary>
public record PerformanceListItem(Guid PerformanceId, string EventName, DateTime StartsAt);

public interface IPerformanceQueryRepository : IQueryRepository<Performance>
{
    Task<List<PerformanceAvailabilityResponse>> AvailabilityForEvent(
        Guid eventId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task<List<PerformanceListItem>> ListAll(CancellationToken cancellationToken = default);
}
