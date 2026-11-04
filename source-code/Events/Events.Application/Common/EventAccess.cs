using Events.Domain.Models.Events;
using Events.Domain.Models.Teams;
using Events.Domain.Repositories;

namespace Events.Application.Common;

/// <summary>
/// EVT-R1 — scope (ADR-001). Checks the caller against Events' own copy of the Teams. Someone not on the
/// Organizer's Team gets a 404, so a Draft or a Venue is not revealed; someone on it without the needed
/// Team role gets a 403.
/// </summary>
public class EventAccess
{
    private readonly ITeamMemberDomainRepository members;
    private readonly IEventDomainRepository events;
    private readonly ISaleDomainRepository sales;
    private readonly ICurrentUser user;

    public EventAccess(
        ITeamMemberDomainRepository members,
        IEventDomainRepository events,
        ISaleDomainRepository sales,
        ICurrentUser user)
    {
        this.members = members;
        this.events = events;
        this.sales = sales;
        this.user = user;
    }

    public async Task<TeamRole> Require(
        Guid organizerId,
        string what,
        object id,
        CancellationToken cancellationToken)
    {
        var userId = user.UserId ?? throw new ForbiddenException("Sign in first.");

        var member = await members.Find(organizerId, userId, cancellationToken)
                     ?? throw new NotFoundException(what, id);

        return member.TeamRole;
    }

    /// <summary>Loads one of the Organizer's Events, tracked, for someone on its Team.</summary>
    public async Task<Event> LoadEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await events.Find(eventId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Event), eventId);

        await Require(@event.OrganizerId, nameof(Event), eventId, cancellationToken);

        return @event;
    }

    /// <summary>EVT-R8: what has sales. A Draft has none, so nothing is looked up for it.</summary>
    public async Task<SalesFigures> FiguresFor(Event @event, CancellationToken cancellationToken)
        => @event.IsPublished
            ? await sales.FiguresFor(@event.Performances.Select(p => p.Id).ToList(), cancellationToken)
            : SalesFigures.None;
}
