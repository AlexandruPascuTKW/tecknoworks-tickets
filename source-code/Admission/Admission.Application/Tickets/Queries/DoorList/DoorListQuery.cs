using Admission.Application.Common;

namespace Admission.Application.Tickets.Queries.DoorList;

/// <summary>
/// ADM-V2 — See a Performance's Door list. Manager or Owner of the Organizer whose Performance it is
/// (ADM-R1); the id is the Performance's.
/// </summary>
public class DoorListQuery : EntityCommand, IRequest<DoorListResponse>
{
    public class DoorListQueryHandler : IRequestHandler<DoorListQuery, DoorListResponse>
    {
        private readonly ITicketQueryRepository tickets;
        private readonly TeamAccess access;

        public DoorListQueryHandler(ITicketQueryRepository tickets, TeamAccess access)
        {
            this.tickets = tickets;
            this.access = access;
        }

        public async Task<DoorListResponse> Handle(DoorListQuery request, CancellationToken cancellationToken)
        {
            var doorList = await tickets.DoorList(request.Id, cancellationToken)
                           ?? throw new NotFoundException("Performance", request.Id);

            await access.Require(doorList.OrganizerId, "Performance", request.Id, cancellationToken);

            return doorList;
        }
    }
}
