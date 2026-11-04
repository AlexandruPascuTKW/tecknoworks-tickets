using Events.Domain.Models.Events;
using Events.Domain.Models.Venues;

namespace Events.Domain.Factories;

public interface IEventFactory : IFactory<Event>
{
    IEventFactory ForOrganizer(Guid organizerId);
    IEventFactory WithName(string name);
    IEventFactory WithDescription(string? description);
    IEventFactory WithImage(Guid imageId);
    IEventFactory AtVenue(Venue venue);
}
