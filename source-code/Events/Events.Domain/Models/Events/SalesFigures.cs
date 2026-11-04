namespace Events.Domain.Models.Events;

/// <summary>
/// Tickets on Confirmed or Pending payment Orders, per Performance and Ticket type, as Events knows them
/// from Sales' events. Fed into the EVT-R8 checks; Events never asks Sales.
/// </summary>
public class SalesFigures : ValueObject
{
    private readonly IReadOnlyDictionary<(Guid PerformanceId, Guid TicketTypeId), int> tickets;

    private SalesFigures(IReadOnlyDictionary<(Guid, Guid), int> tickets) => this.tickets = tickets;

    public static SalesFigures None { get; } = new(new Dictionary<(Guid, Guid), int>());

    public static SalesFigures From(IEnumerable<(Guid PerformanceId, Guid TicketTypeId, int NumberOfTickets)> lines)
        => new(lines
            .GroupBy(l => (l.PerformanceId, l.TicketTypeId))
            .ToDictionary(g => g.Key, g => g.Sum(l => l.NumberOfTickets)));

    /// <summary>A Performance has sales when it has tickets on such an Order.</summary>
    public bool PerformanceHasSales(Guid performanceId)
        => tickets.Any(t => t.Key.PerformanceId == performanceId && t.Value > 0);

    public bool TicketTypeHasSales(Guid ticketTypeId)
        => tickets.Any(t => t.Key.TicketTypeId == ticketTypeId && t.Value > 0);

    /// <summary>The most tickets of this type on such Orders for any one Performance (EVT-R8, Quantity down).</summary>
    public int MostSoldOrHeld(Guid ticketTypeId)
        => tickets.Where(t => t.Key.TicketTypeId == ticketTypeId).Select(t => t.Value).DefaultIfEmpty(0).Max();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var entry in tickets.OrderBy(t => t.Key))
        {
            yield return entry.Key.PerformanceId;
            yield return entry.Key.TicketTypeId;
            yield return entry.Value;
        }
    }
}
