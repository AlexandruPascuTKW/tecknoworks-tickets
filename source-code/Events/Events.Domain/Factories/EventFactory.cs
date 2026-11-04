using Events.Domain.Models.Events;
using Events.Domain.Models.Venues;

namespace Events.Domain.Factories;

/// <summary>EVT-R3 (starts as Draft) and EVT-R4 (at one of the Organizer's own Venues).</summary>
internal class EventFactory : IEventFactory
{
    private Guid? organizerId;
    private string? name;
    private string? description;
    private Guid? imageId;
    private Venue? venue;

    public IEventFactory ForOrganizer(Guid organizerId)
    {
        this.organizerId = organizerId;
        return this;
    }

    public IEventFactory WithName(string name)
    {
        this.name = name;
        return this;
    }

    public IEventFactory WithDescription(string? description)
    {
        this.description = description;
        return this;
    }

    public IEventFactory WithImage(Guid imageId)
    {
        this.imageId = imageId;
        return this;
    }

    public IEventFactory AtVenue(Venue venue)
    {
        this.venue = venue;
        return this;
    }

    public Event Build()
    {
        if (organizerId is null || name is null || venue is null)
        {
            throw new InvalidOperationException("An Event needs an Organizer, a name and a Venue.");
        }

        return new Event(organizerId.Value, name, description, imageId, venue);
    }
}
