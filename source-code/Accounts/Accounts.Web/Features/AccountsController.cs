using Accounts.Application.Accounts.Commands.AcceptInvitation;
using Accounts.Application.Accounts.Commands.Register;
using Accounts.Application.Accounts.Commands.SignIn;
using Accounts.Application.Accounts.Queries.Me;

namespace Accounts.Web.Features;

[Authorize]
public class AccountsController : ApiController
{
    /// <summary>ACC-A1 — Register.</summary>
    [HttpPost, AllowAnonymous]
    public async Task<ActionResult<SignInResponse>> Register(RegisterCommand command)
        => await Send(command);

    /// <summary>ACC-A2 — Sign in.</summary>
    [HttpPost, AllowAnonymous]
    public async Task<ActionResult<SignInResponse>> SignIn(SignInCommand command)
        => await Send(command);

    /// <summary>ACC-R3 — choose a Password from an invitation link.</summary>
    [HttpPost, AllowAnonymous]
    public async Task<ActionResult> AcceptInvitation(AcceptInvitationCommand command)
        => await Send(command);

    [HttpGet]
    public async Task<ActionResult<CurrentAccountResponse>> Me()
        => await Send(new CurrentAccountQuery());
}
