using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;

namespace Sales.Domain.Repositories;

public interface IPerformanceDomainRepository : IDomainRepository<Performance>
{
    /// <summary>Loads the Performance with its Ticket types, tracked, so its Version can guard the save.</summary>
    Task<Performance?> FindByPerformanceId(Guid performanceId, CancellationToken cancellationToken = default);

    Task<List<Performance>> ForEvent(Guid eventId, CancellationToken cancellationToken = default);

    Task Delete(Performance performance, CancellationToken cancellationToken = default);
}

public interface IOrderDomainRepository : IDomainRepository<Order>
{
    /// <remarks>
    /// <c>Save</c> writes the Order together with everything else changed in this request — one transaction —
    /// and throws <see cref="Exceptions.ConcurrencyConflictException"/> if a Performance changed meanwhile.
    /// </remarks>
    Task<Order?> Find(Guid id, CancellationToken cancellationToken = default);

    Task<Order?> FindByPaymentReference(string providerReference, CancellationToken cancellationToken = default);

    /// <summary>SAL-R15: the Attendee's Orders still Pending payment for this Performance, tracked.</summary>
    Task<List<Order>> PendingFor(Guid accountId, Guid performanceId, CancellationToken cancellationToken = default);

    /// <summary>SAL-R5: Pending payment Orders whose Hold has run out, tracked.</summary>
    Task<List<Order>> Lapsed(DateTime nowUtc, CancellationToken cancellationToken = default);

    /// <summary>SAL-R6: tickets on Confirmed Orders and on Pending payment Orders whose Hold has not run out.</summary>
    Task<TicketCounts> ActiveTicketCounts(
        Guid performanceId,
        DateTime nowUtc,
        IReadOnlyCollection<Guid> excludedOrderIds,
        CancellationToken cancellationToken = default);
}
