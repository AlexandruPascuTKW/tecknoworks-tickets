using Events.Application.Common;

namespace Events.Application.Events.Queries.TeamDetails;

/// <summary>EVT-V3 — See an Event as its Team: the whole Event, Draft or Published.</summary>
public class EventTeamDetailsQuery : EntityCommand, IRequest<EventTeamResponse>
{
    public class EventTeamDetailsQueryHandler : IRequestHandler<EventTeamDetailsQuery, EventTeamResponse>
    {
        private readonly IEventQueryRepository events;
        private readonly EventAccess access;

        public EventTeamDetailsQueryHandler(IEventQueryRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<EventTeamResponse> Handle(EventTeamDetailsQuery request, CancellationToken cancellationToken)
        {
            var response = await events.TeamDetails(request.Id, cancellationToken)
                           ?? throw new NotFoundException("Event", request.Id);

            await access.Require(response.OrganizerId, "Event", request.Id, cancellationToken);

            return response;
        }
    }
}
