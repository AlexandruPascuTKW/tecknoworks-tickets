using Accounts.Application.Common;
using Accounts.Domain.Models.Organizers;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Organizers.Queries.Team;

/// <summary>ACC-V2 — See the Team. Owners and Managers of that Organizer.</summary>
public class OrganizerTeamQuery : EntityCommand, IRequest<List<TeamMemberResponse>>
{
    public class OrganizerTeamQueryHandler : IRequestHandler<OrganizerTeamQuery, List<TeamMemberResponse>>
    {
        private readonly IOrganizerDomainRepository organizers;
        private readonly IOrganizerQueryRepository queries;
        private readonly ICurrentUser user;

        public OrganizerTeamQueryHandler(
            IOrganizerDomainRepository organizers,
            IOrganizerQueryRepository queries,
            ICurrentUser user)
        {
            this.organizers = organizers;
            this.queries = queries;
            this.user = user;
        }

        public async Task<List<TeamMemberResponse>> Handle(OrganizerTeamQuery request, CancellationToken cancellationToken)
        {
            var organizer = await organizers.Find(request.Id, cancellationToken)
                            ?? throw new NotFoundException(nameof(Organizer), request.Id);

            TeamAccess.Require(organizer, user);

            return await queries.Team(request.Id, cancellationToken);
        }
    }
}
