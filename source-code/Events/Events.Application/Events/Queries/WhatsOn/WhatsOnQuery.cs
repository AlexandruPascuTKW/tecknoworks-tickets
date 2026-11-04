namespace Events.Application.Events.Queries.WhatsOn;

/// <summary>EVT-V4 — Find what's on: the Published Events. Visitors and Attendees (EVT-R1).</summary>
public class WhatsOnQuery : IRequest<List<WhatsOnListItem>>
{
    public class WhatsOnQueryHandler : IRequestHandler<WhatsOnQuery, List<WhatsOnListItem>>
    {
        private readonly IEventQueryRepository events;

        public WhatsOnQueryHandler(IEventQueryRepository events) => this.events = events;

        public async Task<List<WhatsOnListItem>> Handle(WhatsOnQuery request, CancellationToken cancellationToken)
            => await events.WhatsOn(cancellationToken);
    }
}
