using Events.Application.Events;
using Events.Application.Events.Queries.Image;
using Events.Application.Events.Queries.PublicDetails;
using Events.Application.Events.Queries.WhatsOn;

namespace Events.Web.Features;

/// <summary>What the public sees: Published Events (EVT-V4, EVT-V5) and their pictures. No sign-in needed.</summary>
[AllowAnonymous]
public class WhatsOnController : ApiController
{
    /// <summary>EVT-V4 — Find what's on.</summary>
    [HttpGet]
    public async Task<ActionResult<List<WhatsOnListItem>>> List()
        => await Send(new WhatsOnQuery());

    /// <summary>EVT-V5 — See an Event.</summary>
    [HttpGet(Id)]
    public async Task<ActionResult<PublicEventResponse>> Details([FromRoute] PublicEventQuery query)
        => await Send(query);

    /// <summary>ADR-008 — the picture; cached for a long time once the Event is Published.</summary>
    [HttpGet(Id)]
    public async Task<IActionResult> Image([FromRoute] EventImageQuery query)
    {
        var image = await Mediator.Send(query);

        Response.Headers.CacheControl = image.IsPublished
            ? "public, max-age=2592000"
            : "private, no-store";

        return File(image.Content, image.ContentType);
    }
}
