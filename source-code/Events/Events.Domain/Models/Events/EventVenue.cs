namespace Events.Domain.Models.Events;

/// <summary>
/// What the Event knows of its Venue, copied when the Event is created or its Venue is changed. The
/// capacity rule (EVT-R6) is then checked inside the Event alone. Safe while a Venue cannot be changed.
/// </summary>
public class EventVenue : ValueObject
{
    private EventVenue()
    {
        VenueName = default!;
        Address = default!;
    }

    public EventVenue(Guid venueId, string venueName, string address, int capacity)
    {
        VenueId = venueId;
        VenueName = venueName;
        Address = address;
        Capacity = capacity;
    }

    public Guid VenueId { get; private set; }

    public string VenueName { get; private set; }

    public string Address { get; private set; }

    public int Capacity { get; private set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return VenueId;
        yield return VenueName;
        yield return Address;
        yield return Capacity;
    }
}
