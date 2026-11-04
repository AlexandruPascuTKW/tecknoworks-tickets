using Events.Domain.Models.Venues;

namespace Events.Domain.Factories;

internal class VenueFactory : IVenueFactory
{
    private Guid? organizerId;
    private string? venueName;
    private string? address;
    private int? capacity;

    public IVenueFactory ForOrganizer(Guid organizerId)
    {
        this.organizerId = organizerId;
        return this;
    }

    public IVenueFactory WithName(string venueName)
    {
        this.venueName = venueName;
        return this;
    }

    public IVenueFactory WithAddress(string address)
    {
        this.address = address;
        return this;
    }

    public IVenueFactory WithCapacity(int capacity)
    {
        this.capacity = capacity;
        return this;
    }

    public Venue Build()
    {
        if (organizerId is null || venueName is null || address is null || capacity is null)
        {
            throw new InvalidOperationException("A Venue needs an Organizer, a name, an address and a capacity.");
        }

        return new Venue(organizerId.Value, venueName, address, capacity.Value);
    }
}
