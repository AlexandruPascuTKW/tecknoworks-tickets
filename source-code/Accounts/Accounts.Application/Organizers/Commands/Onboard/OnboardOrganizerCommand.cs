using Accounts.Application.Services;
using Accounts.Application.Settings;
using Accounts.Domain.Exceptions;
using Accounts.Domain.Factories;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Accounts.Application.Organizers.Commands.Onboard;

/// <summary>ACC-A3 — Onboard an Organizer. The Name and Email address are the first Owner's.</summary>
public class OnboardOrganizerCommand : IRequest<OnboardOrganizerResponse>
{
    public string OrganizerName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string Email { get; set; } = string.Empty;

    public class OnboardOrganizerCommandHandler : IRequestHandler<OnboardOrganizerCommand, OnboardOrganizerResponse>
    {
        private readonly IAccountDomainRepository accounts;
        private readonly IOrganizerDomainRepository organizers;
        private readonly IAccountFactory accountFactory;
        private readonly IOrganizerFactory organizerFactory;
        private readonly IInvitationSender invitations;
        private readonly AccountsSettings settings;
        private readonly TimeProvider clock;

        public OnboardOrganizerCommandHandler(
            IAccountDomainRepository accounts,
            IOrganizerDomainRepository organizers,
            IAccountFactory accountFactory,
            IOrganizerFactory organizerFactory,
            IInvitationSender invitations,
            IOptions<AccountsSettings> settings,
            TimeProvider clock)
        {
            this.accounts = accounts;
            this.organizers = organizers;
            this.accountFactory = accountFactory;
            this.organizerFactory = organizerFactory;
            this.invitations = invitations;
            this.settings = settings.Value;
            this.clock = clock;
        }

        public async Task<OnboardOrganizerResponse> Handle(
            OnboardOrganizerCommand request,
            CancellationToken cancellationToken)
        {
            // ACC-R4: an existing Account becomes the Owner; otherwise one is created for them (ACC-R3).
            // The Account is saved first, so a retry after a failed second save finds it and carries on.
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

            var organizer = organizerFactory
                .WithName(request.OrganizerName)
                .WithFirstOwner(account.Id)
                .Build();

            await organizers.Save(organizer, cancellationToken);

            if (token is not null)
            {
                await invitations.Send(
                    account.Email,
                    account.Name,
                    $"{settings.InvitationLinkBase}?token={token}",
                    cancellationToken);
            }

            return new OnboardOrganizerResponse(organizer.Id);
        }
    }
}
