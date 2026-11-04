using Events.Application.Common;
using Events.Domain.Models.Events;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.ChangeTicketType;

/// <summary>EVT-A8 — Change a Ticket type's name, Price or Quantity (EVT-R6, EVT-R8).</summary>
public class ChangeTicketTypeCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public Guid TicketTypeId { get; set; }

    public string? TicketTypeName { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public class ChangeTicketTypeCommandHandler : IRequestHandler<ChangeTicketTypeCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;

        public ChangeTicketTypeCommandHandler(IEventDomainRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<Result> Handle(ChangeTicketTypeCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);
            var sales = await access.FiguresFor(@event, cancellationToken);

            @event.ChangeTicketType(
                request.TicketTypeId,
                request.TicketTypeName,
                request.Price is { } price ? new Price(price) : null,
                request.Quantity,
                sales);

            await events.Save(@event, cancellationToken);

            return Result.Success;
        }
    }
}
