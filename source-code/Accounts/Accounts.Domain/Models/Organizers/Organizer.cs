using Accounts.Domain.Exceptions;

namespace Accounts.Domain.Models.Organizers;

/// <summary>An organisation that puts events on sale, and its Team (ACC-R4, ACC-R5, ACC-R6).</summary>
public class Organizer : Entity, IAggregateRoot
{
    private readonly List<TeamMember> teamMembers = new();

    /// <summary>ACC-R4: an Organizer never exists without an Owner, so the first Owner is part of creation.</summary>
    internal Organizer(string organizerName, Guid firstOwnerAccountId)
    {
        Guard.ForStringLength(organizerName, 1, OrganizerModelConstants.Organizer.MaxNameLength, nameof(organizerName));

        OrganizerName = organizerName.Trim();
        Join(firstOwnerAccountId, TeamRole.Owner);
    }

    private Organizer()
    {
        OrganizerName = default!;
    }

    public string OrganizerName { get; private set; }

    public IReadOnlyCollection<TeamMember> TeamMembers => teamMembers.AsReadOnly();

    public TeamRole? TeamRoleOf(Guid accountId)
        => teamMembers.FirstOrDefault(m => m.AccountId == accountId)?.TeamRole;

    /// <summary>ACC-R5: either Team role can be given; nobody is added twice.</summary>
    public Organizer AddTeamMember(Guid accountId, TeamRole teamRole)
    {
        if (teamMembers.Any(m => m.AccountId == accountId))
        {
            throw new InvalidOrganizerException("This person is already on the Team.");
        }

        Join(accountId, teamRole);
        return this;
    }

    /// <summary>ACC-R6: the last Owner is never taken off the Team.</summary>
    public Organizer RemoveTeamMember(Guid accountId)
    {
        var member = Find(accountId);

        if (member.TeamRole == TeamRole.Owner && OwnerCount() == 1)
        {
            throw new InvalidOrganizerException("An organizer must keep at least one owner.");
        }

        teamMembers.Remove(member);
        RaiseEvent(() => new TeamMemberRemovedEvent(Id, accountId));
        return this;
    }

    /// <summary>ACC-R6: the last Owner is never made a Manager.</summary>
    public Organizer ChangeTeamRole(Guid accountId, TeamRole teamRole)
    {
        var member = Find(accountId);

        if (member.TeamRole == teamRole)
        {
            return this;
        }

        if (member.TeamRole == TeamRole.Owner && OwnerCount() == 1)
        {
            throw new InvalidOrganizerException("An organizer must keep at least one owner.");
        }

        member.TeamRole = teamRole;
        RaiseEvent(() => new TeamRoleChangedEvent(Id, accountId, teamRole.Name));
        return this;
    }

    private void Join(Guid accountId, TeamRole teamRole)
    {
        teamMembers.Add(new TeamMember(accountId, teamRole));
        RaiseEvent(() => new TeamMemberAddedEvent(Id, accountId, teamRole.Name));
    }

    private TeamMember Find(Guid accountId)
        => teamMembers.FirstOrDefault(m => m.AccountId == accountId)
           ?? throw new InvalidOrganizerException("This person is not on the Team.");

    private int OwnerCount() => teamMembers.Count(m => m.TeamRole == TeamRole.Owner);
}
