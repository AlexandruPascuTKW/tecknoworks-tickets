using Sales.Domain.Exceptions;
using Sales.Domain.Models.Orders;

namespace Sales.Domain.Models.Performances;

/// <summary>
/// Sales' own view of one published Performance: what it offers and at what Price. It is where "never
/// more than the Performance offers" (SAL-R6) is enforced. Every change to what a Performance can sell —
/// a checkout, a late Payment taking tickets back, Events changing a Quantity — moves <see cref="Version"/>
/// on, a concurrency token, so two of them for the same Performance can never both win (ADR-002).
/// </summary>
public class Performance : Entity, IAggregateRoot
{
    private readonly List<OfferedTicketType> offeredTicketTypes = new();

    internal Performance(Guid performanceId, Guid eventId, Guid organizerId, string eventName, DateTime startsAt)
    {
        PerformanceId = performanceId;
        EventId = eventId;
        OrganizerId = organizerId;
        EventName = eventName;
        StartsAt = startsAt;
    }

    private Performance()
    {
        EventName = default!;
    }

    /// <summary>The Performance's id as Events publishes it — the id the rest of the platform uses.</summary>
    public Guid PerformanceId { get; private set; }

    public Guid EventId { get; private set; }

    public Guid OrganizerId { get; private set; }

    public string EventName { get; private set; }

    /// <summary>Date and Start time, local at the Venue (ADR-006).</summary>
    public DateTime StartsAt { get; private set; }

    /// <summary>ADR-002: concurrency token, moved on by every change to what can be sold.</summary>
    public long Version { get; private set; }

    public IReadOnlyCollection<OfferedTicketType> OfferedTicketTypes => offeredTicketTypes.AsReadOnly();

    /// <summary>Takes the latest published state of the Event (EVT-R8 changes arrive this way).</summary>
    public Performance Apply(string eventName, DateTime startsAt, IEnumerable<OfferedTicketTypeData> ticketTypes)
    {
        EventName = eventName;
        StartsAt = startsAt;

        var published = ticketTypes.ToList();

        offeredTicketTypes.RemoveAll(o => published.All(p => p.TicketTypeId != o.TicketTypeId));

        foreach (var data in published)
        {
            var existing = offeredTicketTypes.FirstOrDefault(o => o.TicketTypeId == data.TicketTypeId);

            if (existing is null)
            {
                offeredTicketTypes.Add(new OfferedTicketType(
                    data.TicketTypeId, data.Name, new Money(data.Price), data.Quantity, data.OnSale));
            }
            else
            {
                existing.Replace(data.Name, new Money(data.Price), data.Quantity, data.OnSale);
            }
        }

        Version++;
        return this;
    }

    /// <summary>SAL-R6: Quantity less the tickets sold or held. Never below zero.</summary>
    public int AvailabilityOf(Guid ticketTypeId, TicketCounts active)
    {
        var offered = offeredTicketTypes.FirstOrDefault(o => o.TicketTypeId == ticketTypeId);

        return offered is null ? 0 : Math.Max(0, offered.Quantity - active.Of(ticketTypeId));
    }

    /// <summary>SAL-R6: Sold out when no Availability is left, sold or only held.</summary>
    public bool IsTicketTypeSoldOut(Guid ticketTypeId, TicketCounts active)
        => AvailabilityOf(ticketTypeId, active) == 0;

    /// <summary>SAL-R6: the Performance is Sold out when all its On sale Ticket types are.</summary>
    public bool IsSoldOut(TicketCounts active)
    {
        var onSale = offeredTicketTypes.Where(o => o.OnSale).ToList();

        return onSale.Count > 0 && onSale.All(o => IsTicketTypeSoldOut(o.TicketTypeId, active));
    }

    /// <summary>
    /// SAL-R3 / SAL-R6: refuses a checkout that would break a rule, otherwise takes the tickets (Version
    /// moves on). The Order's own limit of eight (SAL-R4) is the Order's.
    /// </summary>
    public Performance Reserve(
        IReadOnlyCollection<(Guid TicketTypeId, int NumberOfTickets)> lines,
        TicketCounts active,
        DateTime nowAtVenue)
    {
        // SAL-R3: the Performance's Date and Start time have passed.
        if (StartsAt <= nowAtVenue)
        {
            throw new CheckoutRefusedException("Tickets can no longer be bought for this performance.");
        }

        foreach (var line in lines)
        {
            var offered = offeredTicketTypes.FirstOrDefault(o => o.TicketTypeId == line.TicketTypeId)
                          ?? throw new CheckoutRefusedException("This ticket type is not part of the performance.");

            // SAL-R3: a Ticket type chosen is not On sale.
            if (!offered.OnSale)
            {
                throw new CheckoutRefusedException($"{offered.Name} is not on sale.");
            }

            // SAL-R3 / SAL-R6 / SAL-R14: never say how many are left, only whether they are.
            var available = AvailabilityOf(line.TicketTypeId, active);
            if (line.NumberOfTickets > available)
            {
                throw new CheckoutRefusedException(available == 0 ? "Sold out" : "Not enough tickets left");
            }
        }

        Version++;
        return this;
    }

    /// <summary>
    /// SAL-R13: a Payment completes after its Hold ran out. The tickets come back only if the same Ticket
    /// types are still free in the same numbers. Takes them (Version moves on) and says whether it could.
    /// </summary>
    public bool TryTakeBack(IReadOnlyCollection<(Guid TicketTypeId, int NumberOfTickets)> lines, TicketCounts active)
    {
        if (lines.Any(l => l.NumberOfTickets > AvailabilityOf(l.TicketTypeId, active)))
        {
            return false;
        }

        Version++;
        return true;
    }
}
