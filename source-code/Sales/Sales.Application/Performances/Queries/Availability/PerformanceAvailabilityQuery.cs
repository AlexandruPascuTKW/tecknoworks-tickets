using Sales.Application.Common;

namespace Sales.Application.Performances.Queries.Availability;

/// <summary>
/// SAL-V2 — See what's left for the Performances of an Event: whether each Performance, and each of its
/// On sale Ticket types, is Sold out. The public never sees how many are left (SAL-R14).
/// </summary>
public class PerformanceAvailabilityQuery : IRequest<List<PerformanceAvailabilityResponse>>
{
    public Guid EventId { get; set; }

    public class PerformanceAvailabilityQueryHandler
        : IRequestHandler<PerformanceAvailabilityQuery, List<PerformanceAvailabilityResponse>>
    {
        private readonly IPerformanceQueryRepository performances;
        private readonly SalesClock clock;

        public PerformanceAvailabilityQueryHandler(IPerformanceQueryRepository performances, SalesClock clock)
        {
            this.performances = performances;
            this.clock = clock;
        }

        public async Task<List<PerformanceAvailabilityResponse>> Handle(
            PerformanceAvailabilityQuery request,
            CancellationToken cancellationToken)
            => await performances.AvailabilityForEvent(request.EventId, clock.UtcNow, cancellationToken);
    }
}
