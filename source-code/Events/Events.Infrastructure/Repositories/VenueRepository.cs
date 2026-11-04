using Events.Application.Venues;
using Events.Domain.Models.Venues;
using Events.Domain.Repositories;
using Events.Infrastructure.Persistence;

namespace Events.Infrastructure.Repositories;

internal class VenueRepository : DataRepository<EventsDbContext, Venue>, IVenueDomainRepository, IVenueQueryRepository
{
    public VenueRepository(EventsDbContext db) : base(db)
    {
    }

    public async Task<Venue?> Find(Guid id, CancellationToken cancellationToken = default)
        => await All().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<List<VenueListItem>> GetAll(Guid organizerId, CancellationToken cancellationToken = default)
        => await AllAsNoTracking()
            .Where(v => v.OrganizerId == organizerId)
            .OrderBy(v => v.VenueName)
            .Select(v => new VenueListItem(v.Id, v.VenueName, v.Address, v.Capacity))
            .ToListAsync(cancellationToken);
}
