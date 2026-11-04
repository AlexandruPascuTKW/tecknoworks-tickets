using Events.Domain.Factories;
using Events.Domain.Models.Teams;
using Events.Domain.Repositories;

namespace Events.Application.Handlers;

// ADR-001 / ADR-005: Events keeps its own copy of Accounts' Teams. Delivery is at least once, so each
// handler is idempotent: adding twice or removing a missing member changes nothing.

public class TeamMemberAddedEventHandler : IEventHandler<TeamMemberAddedEvent>
{
    private readonly ITeamMemberDomainRepository members;
    private readonly ITeamMemberFactory factory;

    public TeamMemberAddedEventHandler(ITeamMemberDomainRepository members, ITeamMemberFactory factory)
    {
        this.members = members;
        this.factory = factory;
    }

    public async Task Handle(TeamMemberAddedEvent domainEvent)
    {
        var role = Enumeration.FromName<TeamRole>(domainEvent.TeamRole);
        var existing = await members.Find(domainEvent.OrganizerId, domainEvent.AccountId);

        if (existing is not null)
        {
            await members.Save(existing.ChangeRole(role));
            return;
        }

        await members.Save(factory
            .OnTeamOf(domainEvent.OrganizerId)
            .ForAccount(domainEvent.AccountId)
            .WithTeamRole(role)
            .Build());
    }
}

public class TeamMemberRemovedEventHandler : IEventHandler<TeamMemberRemovedEvent>
{
    private readonly ITeamMemberDomainRepository members;

    public TeamMemberRemovedEventHandler(ITeamMemberDomainRepository members) => this.members = members;

    public async Task Handle(TeamMemberRemovedEvent domainEvent)
    {
        var existing = await members.Find(domainEvent.OrganizerId, domainEvent.AccountId);

        if (existing is not null)
        {
            await members.Delete(existing);
        }
    }
}

public class TeamRoleChangedEventHandler : IEventHandler<TeamRoleChangedEvent>
{
    private readonly ITeamMemberDomainRepository members;

    public TeamRoleChangedEventHandler(ITeamMemberDomainRepository members) => this.members = members;

    public async Task Handle(TeamRoleChangedEvent domainEvent)
    {
        var existing = await members.Find(domainEvent.OrganizerId, domainEvent.AccountId);

        if (existing is not null)
        {
            await members.Save(existing.ChangeRole(Enumeration.FromName<TeamRole>(domainEvent.TeamRole)));
        }
    }
}
