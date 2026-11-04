using Events.Domain.Models.Venues;

namespace Events.Domain.Factories;

public interface IVenueFactory : IFactory<Venue>
{
    IVenueFactory ForOrganizer(Guid organizerId);
    IVenueFactory WithName(string venueName);
    IVenueFactory WithAddress(string address);
    IVenueFactory WithCapacity(int capacity);
}
