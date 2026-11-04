namespace Accounts.Application.Organizers.Queries.GetAll;

/// <summary>ACC-V1 — See all Organizers. Platform admin only; enforced by the controller's role check.</summary>
public class GetAllOrganizersQuery : IRequest<List<OrganizerListItem>>
{
    public class GetAllOrganizersQueryHandler : IRequestHandler<GetAllOrganizersQuery, List<OrganizerListItem>>
    {
        private readonly IOrganizerQueryRepository organizers;
        private readonly ICurrentUser user;

        public GetAllOrganizersQueryHandler(IOrganizerQueryRepository organizers, ICurrentUser user)
        {
            this.organizers = organizers;
            this.user = user;
        }

        public async Task<List<OrganizerListItem>> Handle(GetAllOrganizersQuery request, CancellationToken cancellationToken)
        {
            // ACC-V1 Who can: Platform admin. Checked here as well as on the controller.
            if (!user.IsInRole("PlatformAdmin"))
            {
                throw new ForbiddenException("Only a platform admin can see every organizer.");
            }

            return await organizers.GetAll(cancellationToken);
        }
    }
}
