using Accounts.Domain.Models.Organizers;

namespace Accounts.Application.Organizers;

public record TeamMemberResponse(Guid AccountId, string Name, string Email, string TeamRole);

public record OrganizerListItem(Guid Id, string OrganizerName, List<TeamMemberResponse> TeamMembers);

public record MyTeam(Guid OrganizerId, string OrganizerName, string TeamRole);

/// <summary>The read side of Organizers: ACC-V1, ACC-V2 and the Teams of one Account.</summary>
public interface IOrganizerQueryRepository : IQueryRepository<Organizer>
{
    Task<List<OrganizerListItem>> GetAll(CancellationToken cancellationToken = default);

    Task<List<TeamMemberResponse>> Team(Guid organizerId, CancellationToken cancellationToken = default);

    Task<List<MyTeam>> TeamsOf(Guid accountId, CancellationToken cancellationToken = default);
}
