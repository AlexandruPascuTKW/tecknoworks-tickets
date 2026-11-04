using Events.Domain.Exceptions;
using VenueAggregate = Events.Domain.Models.Venues.Venue;

namespace Events.Domain.Models.Events;

/// <summary>
/// A show an Organizer puts on sale, with its Performances and Ticket types (EVT-R3 to EVT-R10).
/// Once Published, every change raises <see cref="PublishedEventUpdatedEvent"/> with the whole snapshot,
/// so Sales and Admission can replace their copy.
/// </summary>
public class Event : Entity, IAggregateRoot
{
    private readonly List<Performance> performances = new();
    private readonly List<TicketType> ticketTypes = new();

    internal Event(Guid organizerId, string name, string? description, Guid? imageId, VenueAggregate venue)
    {
        OrganizerId = organizerId;
        Name = ValidName(name);
        Description = ValidDescription(description);
        ImageId = imageId;
        Venue = CopyOf(venue, organizerId);
        Status = EventStatus.Draft; // EVT-R3: every new Event starts as Draft.
    }

    private Event()
    {
        Name = default!;
        Venue = default!;
        Status = default!;
    }

    public Guid OrganizerId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    /// <summary>The Event's picture, held apart in <c>EventImage</c> so loading an Event stays light (ADR-008).</summary>
    public Guid? ImageId { get; private set; }

    public EventVenue Venue { get; private set; }

    public EventStatus Status { get; private set; }

    public IReadOnlyCollection<Performance> Performances => performances.AsReadOnly();

    public IReadOnlyCollection<TicketType> TicketTypes => ticketTypes.AsReadOnly();

    public bool IsPublished => Status == EventStatus.Published;

    /// <summary>
    /// EVT-A3 / EVT-R8: name, description and image any time; the Venue only while Draft (EVT-R6: never
    /// below what the Ticket types add up to).
    /// </summary>
    public Event ChangeDetails(string? name, string? description, Guid? imageId, VenueAggregate? venue)
    {
        if (venue is not null && venue.Id != Venue.VenueId)
        {
            if (IsPublished)
            {
                throw new InvalidEventException("The venue of a published event can't be changed.");
            }

            var copy = CopyOf(venue, OrganizerId);
            EnsureWithinCapacity(TotalQuantity(), copy.Capacity);
            Venue = copy;
        }

        if (name is not null)
        {
            Name = ValidName(name);
        }

        if (description is not null)
        {
            Description = ValidDescription(description);
        }

        if (imageId is not null)
        {
            ImageId = imageId;
        }

        Changed();
        return this;
    }

    /// <summary>EVT-A4 / EVT-R7: only for a Date and Start time still to come (local time at the Venue).</summary>
    public Performance AddPerformance(DateOnly date, TimeOnly doorsTime, TimeOnly startTime, DateTime nowAtVenue)
    {
        var schedule = new PerformanceSchedule(date, doorsTime, startTime);
        EnsureStillToCome(schedule, nowAtVenue);

        var performance = new Performance(schedule);
        performances.Add(performance);

        Changed();
        return performance;
    }

    /// <summary>EVT-A5 / EVT-R7 / EVT-R8: a Performance with sales can't be moved once Published.</summary>
    public Event ChangePerformance(
        Guid performanceId,
        DateOnly? date,
        TimeOnly? doorsTime,
        TimeOnly? startTime,
        DateTime nowAtVenue,
        SalesFigures sales)
    {
        var performance = FindPerformance(performanceId);

        var schedule = new PerformanceSchedule(
            date ?? performance.Schedule.Date,
            doorsTime ?? performance.Schedule.DoorsTime,
            startTime ?? performance.Schedule.StartTime);

        if (schedule.Equals(performance.Schedule))
        {
            return this;
        }

        if (IsPublished && sales.PerformanceHasSales(performanceId))
        {
            throw new InvalidEventException("This performance has sales and can't be moved.");
        }

        EnsureStillToCome(schedule, nowAtVenue);

        performance.Schedule = schedule;

        Changed();
        return this;
    }

    /// <summary>EVT-A6 / EVT-R8: a Performance with sales can't be removed once Published.</summary>
    public Event RemovePerformance(Guid performanceId, SalesFigures sales)
    {
        var performance = FindPerformance(performanceId);

        if (IsPublished && sales.PerformanceHasSales(performanceId))
        {
            throw new InvalidEventException("This performance has sales and can't be removed.");
        }

        performances.Remove(performance);

        Changed();
        return this;
    }

    /// <summary>EVT-A7 / EVT-R6: allowed while the Quantities stay within the Venue's Capacity.</summary>
    public TicketType AddTicketType(string name, Price price, int quantity)
    {
        var ticketType = new TicketType(name, price, quantity);

        EnsureWithinCapacity(TotalQuantity() + quantity, Venue.Capacity);

        ticketTypes.Add(ticketType);

        Changed();
        return ticketType;
    }

    /// <summary>EVT-A8 / EVT-R6 / EVT-R8: Price any time; name only without sales; Quantity within bounds.</summary>
    public Event ChangeTicketType(Guid ticketTypeId, string? name, Price? price, int? quantity, SalesFigures sales)
    {
        var ticketType = FindTicketType(ticketTypeId);

        var renames = name is not null && name.Trim() != ticketType.Name;
        if (renames && IsPublished && sales.TicketTypeHasSales(ticketTypeId))
        {
            throw new InvalidEventException("This ticket type has sales and can't be renamed.");
        }

        var requantifies = quantity is not null && quantity != ticketType.Quantity;
        if (requantifies)
        {
            if (quantity < ticketType.Quantity && IsPublished && quantity < sales.MostSoldOrHeld(ticketTypeId))
            {
                throw new InvalidEventException("Quantity can't go below the tickets already sold or held.");
            }

            EnsureWithinCapacity(TotalQuantity() - ticketType.Quantity + quantity!.Value, Venue.Capacity);
        }

        if (renames)
        {
            ticketType.Rename(name!);
        }

        if (price is not null)
        {
            ticketType.Reprice(price);
        }

        if (requantifies)
        {
            ticketType.Requantity(quantity!.Value);
        }

        Changed();
        return this;
    }

    /// <summary>EVT-A9 / EVT-R8: one with sales can only be taken off sale (EVT-R10).</summary>
    public Event RemoveTicketType(Guid ticketTypeId, SalesFigures sales)
    {
        var ticketType = FindTicketType(ticketTypeId);

        if (IsPublished && sales.TicketTypeHasSales(ticketTypeId))
        {
            throw new InvalidEventException(
                "This ticket type has sales and can't be removed — take it off sale instead.");
        }

        ticketTypes.Remove(ticketType);

        Changed();
        return this;
    }

    /// <summary>EVT-A11 / EVT-R10: it stays on the Event for the tickets already bought.</summary>
    public Event TakeTicketTypeOffSale(Guid ticketTypeId)
    {
        FindTicketType(ticketTypeId).TakeOffSale();

        Changed();
        return this;
    }

    /// <summary>EVT-A10 / EVT-R3 / EVT-R9: Draft to Published, only with everything needed to sell.</summary>
    public Event Publish()
    {
        if (IsPublished)
        {
            throw new InvalidEventException("This event is already published.");
        }

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(Description))
        {
            missing.Add("a description");
        }

        if (ImageId is null)
        {
            missing.Add("an image");
        }

        if (performances.Count == 0)
        {
            missing.Add("at least one performance");
        }

        if (ticketTypes.Count == 0)
        {
            missing.Add("at least one ticket type");
        }

        if (missing.Count > 0)
        {
            throw new InvalidEventException($"Add {string.Join(" and ", missing)} before publishing.");
        }

        Status = EventStatus.Published;

        RaiseEvent(() => new EventPublishedEvent(
            Id,
            OrganizerId,
            Name,
            Venue.VenueName,
            Venue.Address,
            PublishedPerformances(),
            PublishedTicketTypes()));

        return this;
    }

    private void Changed()
    {
        if (!IsPublished)
        {
            return; // no other context knows a Draft exists
        }

        RaiseEvent(() => new PublishedEventUpdatedEvent(
            Id,
            OrganizerId,
            Name,
            Venue.VenueName,
            Venue.Address,
            PublishedPerformances(),
            PublishedTicketTypes()));
    }

    private PublishedPerformance[] PublishedPerformances()
        => performances
            .Select(p => new PublishedPerformance(p.Id, p.Schedule.Date, p.Schedule.DoorsTime, p.Schedule.StartTime))
            .ToArray();

    private PublishedTicketType[] PublishedTicketTypes()
        => ticketTypes
            .Select(t => new PublishedTicketType(t.Id, t.Name, t.Price.Amount, t.Quantity, t.OnSale))
            .ToArray();

    private int TotalQuantity() => ticketTypes.Sum(t => t.Quantity);

    private static void EnsureWithinCapacity(int total, int capacity)
    {
        if (total > capacity)
        {
            throw new InvalidEventException("Ticket types can't add up to more than the venue's capacity.");
        }
    }

    private static void EnsureStillToCome(PerformanceSchedule schedule, DateTime nowAtVenue)
    {
        if (schedule.StartsAt <= nowAtVenue)
        {
            throw new InvalidEventException("A performance must be on a date and time still to come.");
        }
    }

    // EVT-R4: the Venue belongs to the Event's own Organizer (EVT-R2).
    private static EventVenue CopyOf(VenueAggregate venue, Guid organizerId)
        => venue.OrganizerId == organizerId
            ? new EventVenue(venue.Id, venue.VenueName, venue.Address, venue.Capacity)
            : throw new InvalidEventException("Choose one of your own venues.");

    private Performance FindPerformance(Guid id)
        => performances.FirstOrDefault(p => p.Id == id)
           ?? throw new InvalidEventException("This performance is not part of the event.");

    private TicketType FindTicketType(Guid id)
        => ticketTypes.FirstOrDefault(t => t.Id == id)
           ?? throw new InvalidEventException("This ticket type is not part of the event.");

    private static string ValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > EventModelConstants.Event.MaxNameLength)
        {
            throw new InvalidEventException("Event name is required.");
        }

        return name.Trim();
    }

    private static string? ValidDescription(string? description)
    {
        if (description is not null && description.Length > EventModelConstants.Event.MaxDescriptionLength)
        {
            throw new InvalidEventException("Description is too long.");
        }

        return description;
    }
}
