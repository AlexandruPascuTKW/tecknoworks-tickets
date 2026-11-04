using Accounts.Domain.Models.Organizers;

namespace Accounts.Application.Common;

/// <summary>
/// ACC-R1 — scope. Someone not on the Organizer's Team gets a 404, so the Organizer is not revealed;
/// someone on it without the needed Team role gets a 403 (ADR-001).
/// </summary>
public static class TeamAccess
{
    public static TeamRole Require(Organizer organizer, ICurrentUser user, bool ownerOnly = false)
    {
        var userId = user.UserId ?? throw new ForbiddenException("Sign in first.");

        var role = organizer.TeamRoleOf(userId)
                   ?? throw new NotFoundException(nameof(Organizer), organizer.Id);

        if (ownerOnly && role != TeamRole.Owner)
        {
            throw new ForbiddenException("Only an owner can run the team.");
        }

        return role;
    }
}
