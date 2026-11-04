using Events.Application.Events;
using Events.Domain.Models.Events;
using Events.Domain.Models.Images;
using Events.Domain.Repositories;
using Events.Infrastructure.Persistence;

namespace Events.Infrastructure.Repositories;

/// <summary>One implementation for both sides: the domain repository and the query repository.</summary>
internal class EventRepository : DataRepository<EventsDbContext, Event>, IEventDomainRepository, IEventQueryRepository
{
    public EventRepository(EventsDbContext db) : base(db)
    {
    }

    public async Task<Event?> Find(Guid id, CancellationToken cancellationToken = default)
        => await All()
            .Include(e => e.Performances)
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    // EVT-V2
    public async Task<List<OrganizerEventListItem>> ForOrganizer(
        Guid organizerId,
        string? eventStatus,
        CancellationToken cancellationToken = default)
    {
        var events = await AllAsNoTracking()
            .Include(e => e.Performances)
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync(cancellationToken);

        return events
            .Where(e => eventStatus is null || e.Status.Name == eventStatus)
            .OrderBy(e => e.Name)
            .Select(e => new OrganizerEventListItem(
                e.Id,
                e.Name,
                e.Venue.VenueName,
                e.Status.Name,
                Dates(e)))
            .ToList();
    }

    // EVT-V3
    public async Task<EventTeamResponse?> TeamDetails(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await AllAsNoTracking()
            .Include(x => x.Performances)
            .Include(x => x.TicketTypes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return e is null
            ? null
            : new EventTeamResponse(
                e.Id,
                e.OrganizerId,
                e.Name,
                e.Description,
                e.ImageId,
                e.Venue.VenueId,
                e.Venue.VenueName,
                e.Venue.Address,
                e.Venue.Capacity,
                e.Status.Name,
                Performances(e),
                e.TicketTypes
                    .OrderBy(t => t.Price.Amount)
                    .Select(t => new TicketTypeResponse(t.Id, t.Name, t.Price.Amount, t.Quantity, t.OnSale))
                    .ToList());
    }

    // EVT-V4
    public async Task<List<WhatsOnListItem>> WhatsOn(CancellationToken cancellationToken = default)
    {
        var events = await AllAsNoTracking()
            .Include(e => e.Performances)
            .Where(e => e.Status == EventStatus.Published)
            .ToListAsync(cancellationToken);

        return events
            .OrderBy(e => Dates(e).FirstOrDefault())
            .Select(e => new WhatsOnListItem(e.Id, e.Name, e.ImageId, e.Venue.VenueName, Dates(e)))
            .ToList();
    }

    // EVT-V5
    public async Task<PublicEventResponse?> PublicDetails(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await AllAsNoTracking()
            .Include(x => x.Performances)
            .Include(x => x.TicketTypes)
            .Where(x => x.Status == EventStatus.Published)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return e is null
            ? null
            : new PublicEventResponse(
                e.Id,
                e.Name,
                e.Description,
                e.ImageId,
                e.Venue.VenueName,
                e.Venue.Address,
                Performances(e),
                e.TicketTypes
                    .Where(t => t.OnSale) // EVT-R10: the public no longer sees a Ticket type taken off sale
                    .OrderBy(t => t.Price.Amount)
                    .Select(t => new PublicTicketType(t.Id, t.Name, t.Price.Amount))
                    .ToList());
    }

    public async Task<EventImageResponse?> Image(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await Data.Set<EventImage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);

        var owner = await AllAsNoTracking()
            .Where(e => e.ImageId == imageId)
            .Select(e => new { e.OrganizerId, e.Status })
            .FirstOrDefaultAsync(cancellationToken);

        return image is null || owner is null
            ? null
            : new EventImageResponse(image.Content, image.ContentType, owner.OrganizerId, owner.Status == EventStatus.Published);
    }

    private static List<DateOnly> Dates(Event e)
        => e.Performances.Select(p => p.Schedule.Date).OrderBy(d => d).ToList();

    private static List<PerformanceResponse> Performances(Event e)
        => e.Performances
            .OrderBy(p => p.Schedule.Date)
            .ThenBy(p => p.Schedule.StartTime)
            .Select(p => new PerformanceResponse(p.Id, p.Schedule.Date, p.Schedule.DoorsTime, p.Schedule.StartTime))
            .ToList();
}
