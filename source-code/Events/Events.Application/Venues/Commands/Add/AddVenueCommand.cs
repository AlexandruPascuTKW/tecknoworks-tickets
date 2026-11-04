using Events.Application.Common;
using Events.Domain.Factories;
using Events.Domain.Repositories;

namespace Events.Application.Venues.Commands.Add;

/// <summary>EVT-A1 — Add a Venue. Manager or Owner of the Organizer.</summary>
public class AddVenueCommand : IRequest<AddVenueResponse>
{
    public Guid OrganizerId { get; set; }

    public string VenueName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public class AddVenueCommandHandler : IRequestHandler<AddVenueCommand, AddVenueResponse>
    {
        private readonly IVenueDomainRepository venues;
        private readonly IVenueFactory factory;
        private readonly EventAccess access;

        public AddVenueCommandHandler(IVenueDomainRepository venues, IVenueFactory factory, EventAccess access)
        {
            this.venues = venues;
            this.factory = factory;
            this.access = access;
        }

        public async Task<AddVenueResponse> Handle(AddVenueCommand request, CancellationToken cancellationToken)
        {
            await access.Require(request.OrganizerId, "Organizer", request.OrganizerId, cancellationToken);

            var venue = factory
                .ForOrganizer(request.OrganizerId) // EVT-R2: belongs to this Organizer alone
                .WithName(request.VenueName)
                .WithAddress(request.Address)
                .WithCapacity(request.Capacity)
                .Build();

            await venues.Save(venue, cancellationToken);

            return new AddVenueResponse(venue.Id);
        }
    }
}

public record AddVenueResponse(Guid Id);
