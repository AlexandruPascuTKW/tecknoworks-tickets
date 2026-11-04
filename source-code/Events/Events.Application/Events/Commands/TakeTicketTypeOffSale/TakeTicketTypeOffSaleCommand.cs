using Events.Application.Common;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.TakeTicketTypeOffSale;

/// <summary>EVT-A11 — Take a Ticket type off sale. It stays on the Event for the tickets bought (EVT-R10).</summary>
public class TakeTicketTypeOffSaleCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public Guid TicketTypeId { get; set; }

    public class TakeTicketTypeOffSaleCommandHandler : IRequestHandler<TakeTicketTypeOffSaleCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;

        public TakeTicketTypeOffSaleCommandHandler(IEventDomainRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<Result> Handle(TakeTicketTypeOffSaleCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);

            @event.TakeTicketTypeOffSale(request.TicketTypeId);

            await events.Save(@event, cancellationToken);

            return Result.Success;
        }
    }
}
