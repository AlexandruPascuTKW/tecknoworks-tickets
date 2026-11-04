using Admission.Application.Tickets;
using Admission.Application.Tickets.Queries.DoorList;
using Admission.Application.Tickets.Queries.Mine;

namespace Admission.Web.Features;

[Authorize]
public class TicketsController : ApiController
{
    /// <summary>ADM-V1 — See my Tickets (Attendee).</summary>
    [HttpGet]
    public async Task<ActionResult<List<MyTicketItem>>> Mine()
        => await Send(new MyTicketsQuery());

    /// <summary>ADM-V2 — See a Performance's Door list (Manager, Owner); the id is the Performance's.</summary>
    [HttpGet(Id)]
    public async Task<ActionResult<DoorListResponse>> DoorList([FromRoute] DoorListQuery query)
        => await Send(query);
}
