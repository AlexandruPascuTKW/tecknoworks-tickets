using Events.Domain.Exceptions;

namespace Events.Domain.Models.Venues;

/// <summary>
/// A place where an Organizer holds Performances (EVT-A1). It belongs to the Organizer that added it and
/// is never shared (EVT-R2). It cannot be changed once added, for now (EVT-A1).
/// </summary>
public class Venue : Entity, IAggregateRoot
{
    internal Venue(Guid organizerId, string venueName, string address, int capacity)
    {
        if (string.IsNullOrWhiteSpace(venueName) || venueName.Trim().Length > VenueModelConstants.Venue.MaxNameLength)
        {
            throw new InvalidEventException("Venue name is required.");
        }

        if (string.IsNullOrWhiteSpace(address) || address.Trim().Length > VenueModelConstants.Venue.MaxAddressLength)
        {
            throw new InvalidEventException("Address is required.");
        }

        // EVT-F3: a whole number above zero.
        if (capacity <= 0)
        {
            throw new InvalidEventException("Capacity must be a whole number above zero.");
        }

        OrganizerId = organizerId;
        VenueName = venueName.Trim();
        Address = address.Trim();
        Capacity = capacity;
    }

    private Venue()
    {
        VenueName = default!;
        Address = default!;
    }

    /// <summary>Plain id of the owning Organizer; Accounts owns Organizers, so there is no navigation (EVT-R2).</summary>
    public Guid OrganizerId { get; private set; }

    public string VenueName { get; private set; }

    public string Address { get; private set; }

    public int Capacity { get; private set; }
}
