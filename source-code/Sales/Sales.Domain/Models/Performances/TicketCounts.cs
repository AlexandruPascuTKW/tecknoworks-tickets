namespace Sales.Domain.Models.Performances;

/// <summary>
/// Tickets of each Ticket type on a Performance's Confirmed Orders and on its Pending payment Orders
/// whose Hold has not run out (SAL-R6). The caller works these out from the Orders.
/// </summary>
public class TicketCounts : ValueObject
{
    private readonly IReadOnlyDictionary<Guid, int> counts;

    private TicketCounts(IReadOnlyDictionary<Guid, int> counts) => this.counts = counts;

    public static TicketCounts From(IEnumerable<(Guid TicketTypeId, int NumberOfTickets)> tickets)
        => new(tickets
            .GroupBy(t => t.TicketTypeId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.NumberOfTickets)));

    public int Of(Guid ticketTypeId) => counts.GetValueOrDefault(ticketTypeId);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var entry in counts.OrderBy(c => c.Key))
        {
            yield return entry.Key;
            yield return entry.Value;
        }
    }
}
