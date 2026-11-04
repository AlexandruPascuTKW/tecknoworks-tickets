using Accounts.Application.Common;
using Accounts.Application.Services;
using Accounts.Application.Settings;
using Accounts.Domain.Exceptions;
using Accounts.Domain.Factories;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Models.Organizers;
using Accounts.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Accounts.Application.Organizers.Commands.AddTeamMember;

/// <summary>ACC-A4 — Add a Team member. Only an Owner (ACC-A4 Who can).</summary>
public class AddTeamMemberCommand : IRequest<Result>
{
    public Guid OrganizerId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string TeamRole { get; set; } = string.Empty;

    public string? Name { get; set; }

    public class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, Result>
    {
        private readonly IOrganizerDomainRepository organizers;
        private readonly IAccountDomainRepository accounts;
        private readonly IAccountFactory accountFactory;
        private readonly IInvitationSender invitations;
        private readonly ICurrentUser user;
        private readonly AccountsSettings settings;
        private readonly TimeProvider clock;

        public AddTeamMemberCommandHandler(
            IOrganizerDomainRepository organizers,
            IAccountDomainRepository accounts,
            IAccountFactory accountFactory,
            IInvitationSender invitations,
            ICurrentUser user,
            IOptions<AccountsSettings> settings,
            TimeProvider clock)
        {
            this.organizers = organizers;
            this.accounts = accounts;
            this.accountFactory = accountFactory;
            this.invitations = invitations;
            this.user = user;
            this.settings = settings.Value;
            this.clock = clock;
        }

        public async Task<Result> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var organizer = await organizers.Find(request.OrganizerId, cancellationToken)
                            ?? throw new NotFoundException(nameof(Organizer), request.OrganizerId);

            TeamAccess.Require(organizer, user, ownerOnly: true);

            // ACC-R5: an existing Account joins; otherwise one is created for them (ACC-R3).
            var email = new EmailAddress(request.Email);
            var account = await accounts.FindByEmail(email, cancellationToken);
            string? token = null;

            if (account is null)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    throw new InvalidAccountException("Name is required for someone who has no account yet.");
                }

                var (invitation, plainToken) = Invitation.Issue(clock.GetUtcNow().UtcDateTime, settings.InvitationLifetime);
                token = plainToken;

                account = accountFactory
                    .WithName(request.Name)
                    .WithEmail(email.Value)
                    .WithInvitation(invitation)
                    .Build();

                await accounts.Save(account, cancellationToken);
            }

            organizer.AddTeamMember(account.Id, Enumeration.FromName<TeamRole>(request.TeamRole));

            await organizers.Save(organizer, cancellationToken);

            if (token is not null)
            {
                await invitations.Send(
                    account.Email,
                    account.Name,
                    $"{settings.InvitationLinkBase}?token={token}",
                    cancellationToken);
            }

            return Result.Success;
        }
    }
}
