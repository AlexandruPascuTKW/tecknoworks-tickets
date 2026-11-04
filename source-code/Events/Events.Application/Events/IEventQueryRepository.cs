using Events.Domain.Models.Events;

namespace Events.Application.Events;

public record PerformanceResponse(Guid Id, DateOnly Date, TimeOnly DoorsTime, TimeOnly StartTime);

public record TicketTypeResponse(Guid Id, string TicketTypeName, decimal Price, int Quantity, bool OnSale);

/// <summary>EVT-V2 — one row of the Organizer's Events.</summary>
public record OrganizerEventListItem(
    Guid Id,
    string EventName,
    string VenueName,
    string EventStatus,
    List<DateOnly> PerformanceDates);

/// <summary>EVT-V3 — the whole Event, as its Team sees it.</summary>
public record EventTeamResponse(
    Guid Id,
    Guid OrganizerId,
    string EventName,
    string? Description,
    Guid? ImageId,
    Guid VenueId,
    string VenueName,
    string Address,
    int Capacity,
    string EventStatus,
    List<PerformanceResponse> Performances,
    List<TicketTypeResponse> TicketTypes);

/// <summary>EVT-V4 — one Published Event in what's on.</summary>
public record WhatsOnListItem(
    Guid Id,
    string EventName,
    Guid? ImageId,
    string VenueName,
    List<DateOnly> PerformanceDates);

public record PublicTicketType(Guid Id, string TicketTypeName, decimal Price);

/// <summary>EVT-V5 — one Published Event, as the public sees it.</summary>
public record PublicEventResponse(
    Guid Id,
    string EventName,
    string? Description,
    Guid? ImageId,
    string VenueName,
    string Address,
    List<PerformanceResponse> Performances,
    List<PublicTicketType> TicketTypes);

public record EventImageResponse(byte[] Content, string ContentType, Guid OrganizerId, bool IsPublished);

public interface IEventQueryRepository : IQueryRepository<Event>
{
    Task<List<OrganizerEventListItem>> ForOrganizer(
        Guid organizerId,
        string? eventStatus,
        CancellationToken cancellationToken = default);

    Task<EventTeamResponse?> TeamDetails(Guid id, CancellationToken cancellationToken = default);

    Task<List<WhatsOnListItem>> WhatsOn(CancellationToken cancellationToken = default);

    Task<PublicEventResponse?> PublicDetails(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The picture, with whether the Event that shows it is Published (EVT-R1).</summary>
    Task<EventImageResponse?> Image(Guid imageId, CancellationToken cancellationToken = default);
}
