using Events.Application.Common;

namespace Events.Application.Events.Queries.Team;

/// <summary>EVT-V2 — See the Organizer's Events, Draft or Published. Can filter by Event status.</summary>
public class OrganizerEventsQuery : IRequest<List<OrganizerEventListItem>>
{
    public Guid OrganizerId { get; set; }

    public string? EventStatus { get; set; }

    public class OrganizerEventsQueryHandler : IRequestHandler<OrganizerEventsQuery, List<OrganizerEventListItem>>
    {
        private readonly IEventQueryRepository events;
        private readonly EventAccess access;

        public OrganizerEventsQueryHandler(IEventQueryRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<List<OrganizerEventListItem>> Handle(
            OrganizerEventsQuery request,
            CancellationToken cancellationToken)
        {
            await access.Require(request.OrganizerId, "Organizer", request.OrganizerId, cancellationToken);

            return await events.ForOrganizer(request.OrganizerId, request.EventStatus, cancellationToken);
        }
    }
}
