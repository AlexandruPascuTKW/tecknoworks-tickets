using Accounts.Application.Common;
using Accounts.Domain.Models.Organizers;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Organizers.Commands.RemoveTeamMember;

/// <summary>ACC-A5 — Remove a Team member. Their Account stays, with any tickets they bought.</summary>
public class RemoveTeamMemberCommand : IRequest<Result>
{
    public Guid OrganizerId { get; set; }

    public Guid AccountId { get; set; }

    public class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Result>
    {
        private readonly IOrganizerDomainRepository organizers;
        private readonly ICurrentUser user;

        public RemoveTeamMemberCommandHandler(IOrganizerDomainRepository organizers, ICurrentUser user)
        {
            this.organizers = organizers;
            this.user = user;
        }

        public async Task<Result> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var organizer = await organizers.Find(request.OrganizerId, cancellationToken)
                            ?? throw new NotFoundException(nameof(Organizer), request.OrganizerId);

            TeamAccess.Require(organizer, user, ownerOnly: true);

            organizer.RemoveTeamMember(request.AccountId); // ACC-R6

            await organizers.Save(organizer, cancellationToken);

            return Result.Success;
        }
    }
}
