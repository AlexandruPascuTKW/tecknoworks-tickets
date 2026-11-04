using Events.Application.Common;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.Publish;

/// <summary>EVT-A10 — Publish an Event. Only an Owner: Managers build drafts, the Owner presses the button.</summary>
public class PublishEventCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;

        public PublishEventCommandHandler(IEventDomainRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<Result> Handle(PublishEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await events.Find(request.EventId, cancellationToken)
                         ?? throw new NotFoundException("Event", request.EventId);

            var role = await access.Require(@event.OrganizerId, "Event", request.EventId, cancellationToken);

            if (!role.CanPublish)
            {
                throw new ForbiddenException("Only an owner can publish an event.");
            }

            @event.Publish();

            await events.Save(@event, cancellationToken);

            return Result.Success;
        }
    }
}
