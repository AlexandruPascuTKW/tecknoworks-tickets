using Events.Application.Common;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.ChangePerformance;

/// <summary>EVT-A5 — Move a Performance. Not once it has sales and the Event is Published (EVT-R8).</summary>
public class ChangePerformanceCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public Guid PerformanceId { get; set; }

    public DateOnly? Date { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? DoorsTime { get; set; }

    public class ChangePerformanceCommandHandler : IRequestHandler<ChangePerformanceCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;
        private readonly VenueTime time;

        public ChangePerformanceCommandHandler(IEventDomainRepository events, EventAccess access, VenueTime time)
        {
            this.events = events;
            this.access = access;
            this.time = time;
        }

        public async Task<Result> Handle(ChangePerformanceCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);
            var sales = await access.FiguresFor(@event, cancellationToken);

            @event.ChangePerformance(
                request.PerformanceId,
                request.Date,
                request.DoorsTime,
                request.StartTime,
                time.Now(),
                sales);

            await events.Save(@event, cancellationToken);

            return Result.Success;
        }
    }
}
