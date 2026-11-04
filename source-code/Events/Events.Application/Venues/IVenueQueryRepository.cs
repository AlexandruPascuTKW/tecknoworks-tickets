using Events.Domain.Models.Venues;

namespace Events.Application.Venues;

public record VenueListItem(Guid Id, string VenueName, string Address, int Capacity);

public interface IVenueQueryRepository : IQueryRepository<Venue>
{
    Task<List<VenueListItem>> GetAll(Guid organizerId, CancellationToken cancellationToken = default);
}
