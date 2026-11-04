namespace Events.Domain.Models.Sales;

/// <summary>
/// Events' own record of one Sales Order: which tickets it holds for which Performance, and whether it
/// still counts as a sale (Confirmed or Pending payment). Kept up to date from Sales' events so EVT-R8
/// can tell whether a Performance or Ticket type "has sales" without asking Sales.
/// </summary>
public class Sale : Entity, IAggregateRoot
{
    private readonly List<SaleLine> lines = new();

    internal Sale(Guid orderId, Guid performanceId, IEnumerable<(Guid TicketTypeId, int NumberOfTickets)> orderLines)
    {
        OrderId = orderId;
        PerformanceId = performanceId;
        HasSales = true;
        lines.AddRange(orderLines.Select(l => new SaleLine(l.TicketTypeId, l.NumberOfTickets)));
    }

    private Sale()
    {
    }

    /// <summary>The Order's id in Sales — a plain Guid, there is no reference across contexts.</summary>
    public Guid OrderId { get; private set; }

    public Guid PerformanceId { get; private set; }

    /// <summary>False once the Order Expired; true again if a late Payment confirms it (SAL-R13).</summary>
    public bool HasSales { get; private set; }

    public IReadOnlyCollection<SaleLine> Lines => lines.AsReadOnly();

    public Sale Release()
    {
        HasSales = false;
        return this;
    }

    public Sale Reinstate()
    {
        HasSales = true;
        return this;
    }
}
