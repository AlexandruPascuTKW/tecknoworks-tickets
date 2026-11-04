using Admission.Application.Tickets;
using Admission.Domain.Models.Performances;
using Admission.Domain.Models.Tickets;
using Admission.Domain.Repositories;
using Admission.Infrastructure.Persistence;

namespace Admission.Infrastructure.Repositories;

/// <summary>One implementation for both sides: the domain repository and the query repository.</summary>
internal class TicketRepository : DataRepository<AdmissionDbContext, Ticket>, ITicketDomainRepository, ITicketQueryRepository
{
    public TicketRepository(AdmissionDbContext db) : base(db)
    {
    }

    public async Task<bool> ExistsForOrder(Guid orderId, CancellationToken cancellationToken = default)
        => await AllAsNoTracking().AnyAsync(t => t.OrderId == orderId, cancellationToken);

    public async Task SaveAll(IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        Data.AddRange(tickets);
        await Data.SaveChangesAsync(cancellationToken);
    }

    // ADM-V1
    public async Task<List<MyTicketItem>> Mine(Guid buyerAccountId, CancellationToken cancellationToken = default)
    {
        var tickets = await AllAsNoTracking()
            .Where(t => t.BuyerAccountId == buyerAccountId)
            .ToListAsync(cancellationToken);

        var performances = await PerformancesFor(tickets, cancellationToken);

        return tickets
            .Where(t => performances.ContainsKey(t.PerformanceId))
            .Select(t => (Ticket: t, Performance: performances[t.PerformanceId]))
            .OrderBy(x => x.Performance.Date)
            .ThenBy(x => x.Performance.StartTime)
            .ThenBy(x => x.Ticket.TicketTypeName)
            .Select(x => new MyTicketItem(
                x.Ticket.Id,
                x.Ticket.Code.Display,
                x.Ticket.TicketTypeName,
                x.Performance.PerformanceId,
                x.Performance.EventName,
                x.Performance.VenueName,
                x.Performance.Date,
                x.Performance.DoorsTime,
                x.Performance.StartTime))
            .ToList();
    }

    // ADM-V2
    public async Task<DoorListResponse?> DoorList(Guid performanceId, CancellationToken cancellationToken = default)
    {
        var performance = await Data.Set<Performance>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PerformanceId == performanceId, cancellationToken);

        if (performance is null)
        {
            return null;
        }

        var tickets = await AllAsNoTracking()
            .Where(t => t.PerformanceId == performanceId)
            .ToListAsync(cancellationToken);

        return new DoorListResponse(
            performance.PerformanceId,
            performance.OrganizerId,
            performance.EventName,
            performance.Date,
            performance.StartTime,
            tickets
                .OrderBy(t => t.TicketTypeName)
                .ThenBy(t => t.BuyersName)
                .ThenBy(t => t.Code.Value)
                .Select(t => new DoorListItem(t.Code.Display, t.TicketTypeName, t.BuyersName))
                .ToList());
    }

    private async Task<Dictionary<Guid, Performance>> PerformancesFor(
        IEnumerable<Ticket> tickets,
        CancellationToken cancellationToken)
    {
        var ids = tickets.Select(t => t.PerformanceId).Distinct().ToList();

        return await Data.Set<Performance>()
            .AsNoTracking()
            .Where(p => ids.Contains(p.PerformanceId))
            .ToDictionaryAsync(p => p.PerformanceId, cancellationToken);
    }
}
