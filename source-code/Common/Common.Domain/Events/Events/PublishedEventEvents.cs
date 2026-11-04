public sealed record PublishedPerformance(Guid PerformanceId, DateOnly Date, TimeOnly DoorsTime, TimeOnly StartTime);

public sealed record PublishedTicketType(Guid TicketTypeId, string Name, decimal Price, int Quantity, bool OnSale);

/// <summary>Raised by Events when an Event is published (EVT-A10). A snapshot of what is on sale.</summary>
public sealed record EventPublishedEvent(
    Guid EventId,
    Guid OrganizerId,
    string EventName,
    string VenueName,
    string VenueAddress,
    PublishedPerformance[] Performances,
    PublishedTicketType[] TicketTypes) : IDomainEvent;

/// <summary>Raised by Events for every change to a Published Event (EVT-R8). The same snapshot again.</summary>
public sealed record PublishedEventUpdatedEvent(
    Guid EventId,
    Guid OrganizerId,
    string EventName,
    string VenueName,
    string VenueAddress,
    PublishedPerformance[] Performances,
    PublishedTicketType[] TicketTypes) : IDomainEvent;
