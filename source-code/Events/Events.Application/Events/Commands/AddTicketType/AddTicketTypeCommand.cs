using Events.Application.Common;
using Events.Domain.Models.Events;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.AddTicketType;

/// <summary>EVT-A7 — Add a Ticket type. Within the Venue's Capacity (EVT-R6).</summary>
public class AddTicketTypeCommand : IRequest<AddTicketTypeResponse>
{
    public Guid EventId { get; set; }

    public string TicketTypeName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public class AddTicketTypeCommandHandler : IRequestHandler<AddTicketTypeCommand, AddTicketTypeResponse>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;

        public AddTicketTypeCommandHandler(IEventDomainRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<AddTicketTypeResponse> Handle(AddTicketTypeCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);

            var ticketType = @event.AddTicketType(request.TicketTypeName, new Price(request.Price), request.Quantity);

            await events.Save(@event, cancellationToken);

            return new AddTicketTypeResponse(ticketType.Id);
        }
    }
}

public record AddTicketTypeResponse(Guid Id);
