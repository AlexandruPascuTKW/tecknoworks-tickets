using Admission.Domain.Models.Performances;
using Admission.Domain.Models.Teams;
using Admission.Domain.Models.Tickets;

namespace Admission.Domain.Repositories;

public interface ITicketDomainRepository : IDomainRepository<Ticket>
{
    /// <summary>ADM-R2: a repeated event for the same Order must issue nothing twice.</summary>
    Task<bool> ExistsForOrder(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>ADM-R2: an Order's Tickets are issued together or not at all.</summary>
    Task SaveAll(IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default);
}

public interface IPerformanceDomainRepository : IDomainRepository<Performance>
{
    Task<List<Performance>> ForEvent(Guid eventId, CancellationToken cancellationToken = default);

    Task Delete(Performance performance, CancellationToken cancellationToken = default);
}

public interface ITeamMemberDomainRepository : IDomainRepository<TeamMember>
{
    Task<TeamMember?> Find(Guid organizerId, Guid accountId, CancellationToken cancellationToken = default);

    Task Delete(TeamMember member, CancellationToken cancellationToken = default);
}
