namespace Events.Application.Events.Queries.PublicDetails;

/// <summary>EVT-V5 — See an Event on sale. A Draft is a 404 to everyone outside its Team (EVT-R1).</summary>
public class PublicEventQuery : EntityCommand, IRequest<PublicEventResponse>
{
    public class PublicEventQueryHandler : IRequestHandler<PublicEventQuery, PublicEventResponse>
    {
        private readonly IEventQueryRepository events;

        public PublicEventQueryHandler(IEventQueryRepository events) => this.events = events;

        public async Task<PublicEventResponse> Handle(PublicEventQuery request, CancellationToken cancellationToken)
            => await events.PublicDetails(request.Id, cancellationToken)
               ?? throw new NotFoundException("Event", request.Id);
    }
}
