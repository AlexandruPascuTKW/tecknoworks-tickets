using Admission.Domain.Repositories;

namespace Admission.Application.Common;

/// <summary>
/// ADM-R1 — a Team member sees only their own Organizer's Tickets. Checked against Admission's own copy of
/// the Teams (ADR-001). Someone not on the Team gets a 404, so the Performance is not revealed.
/// </summary>
public class TeamAccess
{
    private readonly ITeamMemberDomainRepository members;
    private readonly ICurrentUser user;

    public TeamAccess(ITeamMemberDomainRepository members, ICurrentUser user)
    {
        this.members = members;
        this.user = user;
    }

    public async Task Require(Guid organizerId, string what, object id, CancellationToken cancellationToken)
    {
        var userId = user.UserId ?? throw new ForbiddenException("Sign in first.");

        if (await members.Find(organizerId, userId, cancellationToken) is null)
        {
            throw new NotFoundException(what, id);
        }
    }
}
