using Sales.Application.Orders;
using Sales.Domain.Exceptions;
using Sales.Domain.Models.Orders;
using Sales.Domain.Models.Performances;
using Sales.Domain.Repositories;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Repositories;

/// <summary>One implementation for both sides: the domain repository and the query repository.</summary>
internal class OrderRepository : DataRepository<SalesDbContext, Order>, IOrderDomainRepository, IOrderQueryRepository
{
    public OrderRepository(SalesDbContext db) : base(db)
    {
    }

    /// <summary>
    /// One SaveChanges for the Order and every tracked change made with it (the Performance's Version, the
    /// Orders it replaces). If the Performance changed meanwhile the Version check fails (ADR-002): the
    /// tracker is cleared so the caller can reload and try again.
    /// </summary>
    public override async Task Save(Order entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.Save(entity, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            Data.ChangeTracker.Clear();
            throw new ConcurrencyConflictException();
        }
    }

    public async Task<Order?> Find(Guid id, CancellationToken cancellationToken = default)
        => await All()
            .Include(o => o.Lines)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<Order?> FindByPaymentReference(string providerReference, CancellationToken cancellationToken = default)
        => await All()
            .Include(o => o.Lines)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Payments.Any(p => p.ProviderReference == providerReference), cancellationToken);

    public async Task<List<Order>> PendingFor(Guid accountId, Guid performanceId, CancellationToken cancellationToken = default)
        => await All()
            .Where(o => o.Buyer.AccountId == accountId
                        && o.PerformanceId == performanceId
                        && o.Status == OrderStatus.PendingPayment)
            .ToListAsync(cancellationToken);

    public async Task<List<Order>> Lapsed(DateTime nowUtc, CancellationToken cancellationToken = default)
        => await All()
            .Where(o => o.Status == OrderStatus.PendingPayment && o.HoldEndsAt <= nowUtc)
            .ToListAsync(cancellationToken);

    public async Task<TicketCounts> ActiveTicketCounts(
        Guid performanceId,
        DateTime nowUtc,
        IReadOnlyCollection<Guid> excludedOrderIds,
        CancellationToken cancellationToken = default)
    {
        var excluded = excludedOrderIds.ToList();

        var orders = await AllAsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.PerformanceId == performanceId
                        && !excluded.Contains(o.Id)
                        && (o.Status == OrderStatus.Confirmed
                            || (o.Status == OrderStatus.PendingPayment && o.HoldEndsAt > nowUtc)))
            .ToListAsync(cancellationToken);

        return TicketCounts.From(orders.SelectMany(o => o.Lines.Select(l => (l.TicketTypeId, l.NumberOfTickets))));
    }

    // SAL-V3
    public async Task<PerformanceSalesResponse?> PerformanceSales(
        Guid performanceId,
        CancellationToken cancellationToken = default)
    {
        var performance = await Data.Set<Performance>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PerformanceId == performanceId, cancellationToken);

        if (performance is null)
        {
            return null;
        }

        var orders = await AllAsNoTracking()
            .Include(o => o.Lines)
            .Include(o => o.Payments)
            .Where(o => o.PerformanceId == performanceId)
            .ToListAsync(cancellationToken);

        // Confirmed Orders, and Expired ones whose Payment was Refunded (SAL-R13).
        var listed = orders
            .Where(o => o.Status == OrderStatus.Confirmed
                        || (o.Status == OrderStatus.Expired
                            && o.Payments.Any(p => p.Result == PaymentResult.Refunded)))
            .OrderBy(o => o.PlacedAt)
            .ToList();

        var confirmed = listed.Where(o => o.Status == OrderStatus.Confirmed).ToList();

        return new PerformanceSalesResponse(
            performance.PerformanceId,
            performance.EventName,
            performance.StartsAt,
            listed.Select(o => new SoldOrderItem(
                    o.Id,
                    o.Buyer.Name,
                    o.Buyer.Email,
                    o.PlacedAt,
                    o.Status.Name,
                    o.Lines.Select(l => new OrderLineResponse(l.TicketTypeName, l.NumberOfTickets, l.TicketPrice.Amount)).ToList(),
                    o.FaceValue.Amount,
                    o.BookingFee.Amount,
                    o.OrderTotal.Amount,
                    o.LatestPayment?.Result?.Name))
                .ToList(),
            confirmed
                .SelectMany(o => o.Lines)
                .GroupBy(l => l.TicketTypeName)
                .Select(g => new TicketTypeSold(g.Key, g.Sum(l => l.NumberOfTickets)))
                .OrderBy(t => t.TicketTypeName)
                .ToList(),
            confirmed.Sum(o => o.FaceValue.Amount),
            confirmed.Sum(o => o.BookingFee.Amount));
    }
}
