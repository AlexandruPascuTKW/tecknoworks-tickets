using Events.Application.Common;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.RemovePerformance;

/// <summary>EVT-A6 — Remove a Performance. Only while it has no sales once Published (EVT-R8).</summary>
public class RemovePerformanceCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public Guid PerformanceId { get; set; }

    public class RemovePerformanceCommandHandler : IRequestHandler<RemovePerformanceCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly EventAccess access;

        public RemovePerformanceCommandHandler(IEventDomainRepository events, EventAccess access)
        {
            this.events = events;
            this.access = access;
        }

        public async Task<Result> Handle(RemovePerformanceCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);

            @event.RemovePerformance(request.PerformanceId, await access.FiguresFor(@event, cancellationToken));

            await events.Save(@event, cancellationToken);

            return Result.Success;
        }
    }
}
