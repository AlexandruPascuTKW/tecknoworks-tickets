using Accounts.Application.Common;
using Accounts.Domain.Models.Organizers;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Organizers.Commands.ChangeTeamRole;

/// <summary>ACC-A6 — Change a Team member's Team role.</summary>
public class ChangeTeamRoleCommand : IRequest<Result>
{
    public Guid OrganizerId { get; set; }

    public Guid AccountId { get; set; }

    public string TeamRole { get; set; } = string.Empty;

    public class ChangeTeamRoleCommandHandler : IRequestHandler<ChangeTeamRoleCommand, Result>
    {
        private readonly IOrganizerDomainRepository organizers;
        private readonly ICurrentUser user;

        public ChangeTeamRoleCommandHandler(IOrganizerDomainRepository organizers, ICurrentUser user)
        {
            this.organizers = organizers;
            this.user = user;
        }

        public async Task<Result> Handle(ChangeTeamRoleCommand request, CancellationToken cancellationToken)
        {
            var organizer = await organizers.Find(request.OrganizerId, cancellationToken)
                            ?? throw new NotFoundException(nameof(Organizer), request.OrganizerId);

            TeamAccess.Require(organizer, user, ownerOnly: true);

            organizer.ChangeTeamRole(request.AccountId, Enumeration.FromName<TeamRole>(request.TeamRole)); // ACC-R6

            await organizers.Save(organizer, cancellationToken);

            return Result.Success;
        }
    }
}
