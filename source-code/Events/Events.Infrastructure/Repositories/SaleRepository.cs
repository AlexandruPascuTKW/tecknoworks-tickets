using Events.Domain.Models.Events;
using Events.Domain.Models.Sales;
using Events.Domain.Repositories;
using Events.Infrastructure.Persistence;

namespace Events.Infrastructure.Repositories;

internal class SaleRepository : DataRepository<EventsDbContext, Sale>, ISaleDomainRepository
{
    public SaleRepository(EventsDbContext db) : base(db)
    {
    }

    public async Task<Sale?> Find(Guid orderId, CancellationToken cancellationToken = default)
        => await All()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

    public async Task<SalesFigures> FiguresFor(
        IReadOnlyCollection<Guid> performanceIds,
        CancellationToken cancellationToken = default)
    {
        var ids = performanceIds.ToList();

        var sales = await AllAsNoTracking()
            .Include(s => s.Lines)
            .Where(s => s.HasSales && ids.Contains(s.PerformanceId))
            .ToListAsync(cancellationToken);

        return SalesFigures.From(sales.SelectMany(s =>
            s.Lines.Select(l => (s.PerformanceId, l.TicketTypeId, l.NumberOfTickets))));
    }
}
