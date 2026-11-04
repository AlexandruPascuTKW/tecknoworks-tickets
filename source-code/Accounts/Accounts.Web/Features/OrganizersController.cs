using Accounts.Application.Organizers;
using Accounts.Application.Organizers.Commands.AddTeamMember;
using Accounts.Application.Organizers.Commands.ChangeTeamRole;
using Accounts.Application.Organizers.Commands.Onboard;
using Accounts.Application.Organizers.Commands.RemoveTeamMember;
using Accounts.Application.Organizers.Queries.GetAll;
using Accounts.Application.Organizers.Queries.Team;

namespace Accounts.Web.Features;

[Authorize]
public class OrganizersController : ApiController
{
    /// <summary>ACC-A3 — Onboard an Organizer (Platform admin).</summary>
    [HttpPost, Authorize(Roles = "PlatformAdmin")]
    public async Task<ActionResult<OnboardOrganizerResponse>> Onboard(OnboardOrganizerCommand command)
        => await Send(command);

    /// <summary>ACC-V1 — See all Organizers (Platform admin).</summary>
    [HttpGet, Authorize(Roles = "PlatformAdmin")]
    public async Task<ActionResult<List<OrganizerListItem>>> All()
        => await Send(new GetAllOrganizersQuery());

    /// <summary>ACC-V2 — See the Team (Owner, Manager).</summary>
    [HttpGet(Id)]
    public async Task<ActionResult<List<TeamMemberResponse>>> Team([FromRoute] OrganizerTeamQuery query)
        => await Send(query);

    /// <summary>ACC-A4 — Add a Team member (Owner).</summary>
    [HttpPost]
    public async Task<ActionResult> AddTeamMember(AddTeamMemberCommand command)
        => await Send(command);

    /// <summary>ACC-A5 — Remove a Team member (Owner).</summary>
    [HttpPost]
    public async Task<ActionResult> RemoveTeamMember(RemoveTeamMemberCommand command)
        => await Send(command);

    /// <summary>ACC-A6 — Change a Team member's Team role (Owner).</summary>
    [HttpPost]
    public async Task<ActionResult> ChangeTeamRole(ChangeTeamRoleCommand command)
        => await Send(command);
}
