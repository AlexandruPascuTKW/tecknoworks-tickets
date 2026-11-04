using Events.Application.Common;

namespace Events.Application.Venues.Queries.GetAll;

/// <summary>EVT-V1 — See the Organizer's Venues. Manager or Owner of the Organizer.</summary>
public class GetAllVenuesQuery : IRequest<List<VenueListItem>>
{
    public Guid OrganizerId { get; set; }

    public class GetAllVenuesQueryHandler : IRequestHandler<GetAllVenuesQuery, List<VenueListItem>>
    {
        private readonly IVenueQueryRepository venues;
        private readonly EventAccess access;

        public GetAllVenuesQueryHandler(IVenueQueryRepository venues, EventAccess access)
        {
            this.venues = venues;
            this.access = access;
        }

        public async Task<List<VenueListItem>> Handle(GetAllVenuesQuery request, CancellationToken cancellationToken)
        {
            await access.Require(request.OrganizerId, "Organizer", request.OrganizerId, cancellationToken);

            return await venues.GetAll(request.OrganizerId, cancellationToken);
        }
    }
}
