using Events.Application.Events;
using Events.Application.Events.Commands.AddPerformance;
using Events.Application.Events.Commands.AddTicketType;
using Events.Application.Events.Commands.ChangeDetails;
using Events.Application.Events.Commands.ChangePerformance;
using Events.Application.Events.Commands.ChangeTicketType;
using Events.Application.Events.Commands.Create;
using Events.Application.Events.Commands.Publish;
using Events.Application.Events.Commands.RemovePerformance;
using Events.Application.Events.Commands.RemoveTicketType;
using Events.Application.Events.Commands.TakeTicketTypeOffSale;
using Events.Application.Events.Queries.Team;
using Events.Application.Events.Queries.TeamDetails;

namespace Events.Web.Features;

/// <summary>What an Organizer's Team does with its Events (EVT-A2 to EVT-A11, EVT-V2, EVT-V3).</summary>
[Authorize]
public class EventsController : ApiController
{
    [HttpPost]
    public async Task<ActionResult<CreateEventResponse>> Create(CreateEventCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult> ChangeDetails(ChangeEventDetailsCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult<AddPerformanceResponse>> AddPerformance(AddPerformanceCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult> ChangePerformance(ChangePerformanceCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult> RemovePerformance(RemovePerformanceCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult<AddTicketTypeResponse>> AddTicketType(AddTicketTypeCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult> ChangeTicketType(ChangeTicketTypeCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult> RemoveTicketType(RemoveTicketTypeCommand command)
        => await Send(command);

    [HttpPost]
    public async Task<ActionResult> TakeTicketTypeOffSale(TakeTicketTypeOffSaleCommand command)
        => await Send(command);

    /// <summary>EVT-A10 — only an Owner.</summary>
    [HttpPost]
    public async Task<ActionResult> Publish(PublishEventCommand command)
        => await Send(command);

    /// <summary>EVT-V2 — the Organizer's Events, Draft or Published; filter with ?eventStatus=.</summary>
    [HttpGet]
    public async Task<ActionResult<List<OrganizerEventListItem>>> Team([FromQuery] OrganizerEventsQuery query)
        => await Send(query);

    /// <summary>EVT-V3 — one Event as its Team sees it.</summary>
    [HttpGet(Id)]
    public async Task<ActionResult<EventTeamResponse>> TeamDetails([FromRoute] EventTeamDetailsQuery query)
        => await Send(query);
}
