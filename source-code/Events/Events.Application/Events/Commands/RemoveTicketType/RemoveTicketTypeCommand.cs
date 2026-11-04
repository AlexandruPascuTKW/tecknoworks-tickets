using Events.Application.Common;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.RemoveTicketType;

/// <summary>EVT-A9 — Remove a Ticket type. Not one with sales; take it off sale instead (EVT-R8, EVT-R10).</summary>
public class RemoveTicketTypeCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public Guid TicketTypeId { get; set; }

    public class RemoveTicketTypeCommandHandler : IRequestHandler<RemoveTicketTypeCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;

        public RemoveTicketTypeCommandHandler(IEventDomainRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<Result> Handle(RemoveTicketTypeCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);

            @event.RemoveTicketType(request.TicketTypeId, await access.FiguresFor(@event, cancellationToken));

            await events.Save(@event, cancellationToken);

            return Result.Success;
        }
    }
}
