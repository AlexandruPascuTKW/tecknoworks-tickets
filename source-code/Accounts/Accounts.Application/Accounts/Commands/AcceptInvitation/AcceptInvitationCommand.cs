using Accounts.Application.Services;
using Accounts.Domain.Exceptions;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Accounts.Commands.AcceptInvitation;

/// <summary>ACC-R3 — someone whose Account was made for them chooses their own Password.</summary>
public class AcceptInvitationCommand : IRequest<Result>
{
    public string Token { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
    {
        private readonly IAccountDomainRepository accounts;
        private readonly IPasswordHasher hasher;
        private readonly TimeProvider clock;

        public AcceptInvitationCommandHandler(
            IAccountDomainRepository accounts,
            IPasswordHasher hasher,
            TimeProvider clock)
        {
            this.accounts = accounts;
            this.hasher = hasher;
            this.clock = clock;
        }

        public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
        {
            var account = await accounts.FindByInvitationTokenHash(Invitation.HashOf(request.Token), cancellationToken)
                          ?? throw new InvalidAccountException("This invitation is not valid.");

            account.AcceptInvitation(hasher.Hash(request.Password), clock.GetUtcNow().UtcDateTime);

            await accounts.Save(account, cancellationToken);

            return Result.Success;
        }
    }
}
