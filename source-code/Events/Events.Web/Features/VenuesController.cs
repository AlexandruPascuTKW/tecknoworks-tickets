using Events.Application.Venues;
using Events.Application.Venues.Commands.Add;
using Events.Application.Venues.Queries.GetAll;

namespace Events.Web.Features;

[Authorize]
public class VenuesController : ApiController
{
    /// <summary>EVT-A1 — Add a Venue (Manager, Owner).</summary>
    [HttpPost]
    public async Task<ActionResult<AddVenueResponse>> Add(AddVenueCommand command)
        => await Send(command);

    /// <summary>EVT-V1 — See the Organizer's Venues (Manager, Owner).</summary>
    [HttpGet]
    public async Task<ActionResult<List<VenueListItem>>> All([FromQuery] GetAllVenuesQuery query)
        => await Send(query);
}
