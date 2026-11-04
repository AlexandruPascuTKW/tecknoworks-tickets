using Events.Application.Common;

namespace Events.Application.Events.Queries.Image;

/// <summary>EVT-F6 / ADR-008 — an Event's picture. Public once the Event is Published; its Team's before.</summary>
public class EventImageQuery : EntityCommand, IRequest<EventImageResponse>
{
    public class EventImageQueryHandler : IRequestHandler<EventImageQuery, EventImageResponse>
    {
        private readonly IEventQueryRepository events;
        private readonly EventAccess access;

        public EventImageQueryHandler(IEventQueryRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<EventImageResponse> Handle(EventImageQuery request, CancellationToken cancellationToken)
        {
            var image = await events.Image(request.Id, cancellationToken)
                        ?? throw new NotFoundException("Image", request.Id);

            if (!image.IsPublished)
            {
                await access.Require(image.OrganizerId, "Image", request.Id, cancellationToken);
            }

            return image;
        }
    }
}
