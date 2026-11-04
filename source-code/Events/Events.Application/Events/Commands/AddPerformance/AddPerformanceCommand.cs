using Events.Application.Common;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.AddPerformance;

/// <summary>EVT-A4 — Add a Performance. Only for a Date and Start time still to come (EVT-R7).</summary>
public class AddPerformanceCommand : IRequest<AddPerformanceResponse>
{
    public Guid EventId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly DoorsTime { get; set; }

    public class AddPerformanceCommandHandler : IRequestHandler<AddPerformanceCommand, AddPerformanceResponse>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;
        private readonly VenueTime time;

        public AddPerformanceCommandHandler(IEventDomainRepository events, EventAccess access, VenueTime time)
        {
            this.events = events;
            this.access = access;
            this.time = time;
        }

        public async Task<AddPerformanceResponse> Handle(AddPerformanceCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);

            var performance = @event.AddPerformance(request.Date, request.DoorsTime, request.StartTime, time.Now());

            await events.Save(@event, cancellationToken);

            return new AddPerformanceResponse(performance.Id);
        }
    }
}

public record AddPerformanceResponse(Guid Id);
