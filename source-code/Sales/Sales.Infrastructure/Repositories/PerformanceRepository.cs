using Sales.Application.Performances;
using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;
using Sales.Domain.Repositories;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Repositories;

/// <summary>One implementation for both sides: the domain repository and the query repository.</summary>
internal class PerformanceRepository : DataRepository<SalesDbContext, Performance>,
    IPerformanceDomainRepository,
    IPerformanceQueryRepository
{
    public PerformanceRepository(SalesDbContext db) : base(db)
    {
    }

    public async Task<Performance?> FindByPerformanceId(Guid performanceId, CancellationToken cancellationToken = default)
        => await All()
            .Include(p => p.OfferedTicketTypes)
            .FirstOrDefaultAsync(p => p.PerformanceId == performanceId, cancellationToken);

    public async Task<List<Performance>> ForEvent(Guid eventId, CancellationToken cancellationToken = default)
        => await All()
            .Include(p => p.OfferedTicketTypes)
            .Where(p => p.EventId == eventId)
            .ToListAsync(cancellationToken);

    public async Task Delete(Performance performance, CancellationToken cancellationToken = default)
    {
        Data.Remove(performance);
        await Data.SaveChangesAsync(cancellationToken);
    }

    // SAL-V2
    public async Task<List<PerformanceAvailabilityResponse>> AvailabilityForEvent(
        Guid eventId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var performances = await AllAsNoTracking()
            .Include(p => p.OfferedTicketTypes)
            .Where(p => p.EventId == eventId)
            .ToListAsync(cancellationToken);

        var ids = performances.Select(p => p.PerformanceId).ToList();

        // SAL-R6: tickets on Confirmed Orders, and on Pending payment Orders whose Hold has not run out.
        var orders = await Data.Set<Order>()
            .AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => ids.Contains(o.PerformanceId)
                        && (o.Status == OrderStatus.Confirmed
                            || (o.Status == OrderStatus.PendingPayment && o.HoldEndsAt > nowUtc)))
            .ToListAsync(cancellationToken);

        return performances
            .OrderBy(p => p.StartsAt)
            .Select(p =>
            {
                var active = TicketCounts.From(orders
                    .Where(o => o.PerformanceId == p.PerformanceId)
                    .SelectMany(o => o.Lines.Select(l => (l.TicketTypeId, l.NumberOfTickets))));

                return new PerformanceAvailabilityResponse(
                    p.PerformanceId,
                    p.StartsAt,
                    p.IsSoldOut(active),
                    p.OfferedTicketTypes
                        .Where(t => t.OnSale)
                        .Select(t => new TicketTypeAvailability(
                            t.TicketTypeId,
                            t.Name,
                            p.IsTicketTypeSoldOut(t.TicketTypeId, active)))
                        .ToList());
            })
            .ToList();
    }

    public async Task<List<PerformanceListItem>> ListAll(CancellationToken cancellationToken = default)
    {
        var performances = await AllAsNoTracking().ToListAsync(cancellationToken);

        return performances
            .OrderBy(p => p.StartsAt)
            .Select(p => new PerformanceListItem(p.PerformanceId, p.EventName, p.StartsAt))
            .ToList();
    }
}
