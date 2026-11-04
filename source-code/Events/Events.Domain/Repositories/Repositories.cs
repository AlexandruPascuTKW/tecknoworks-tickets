using Events.Domain.Models.Events;
using Events.Domain.Models.Images;
using Events.Domain.Models.Sales;
using Events.Domain.Models.Teams;
using Events.Domain.Models.Venues;

namespace Events.Domain.Repositories;

public interface IVenueDomainRepository : IDomainRepository<Venue>
{
    Task<Venue?> Find(Guid id, CancellationToken cancellationToken = default);
}

public interface IEventDomainRepository : IDomainRepository<Event>
{
    /// <summary>Loads the Event with its Performances and Ticket types, tracked.</summary>
    Task<Event?> Find(Guid id, CancellationToken cancellationToken = default);
}

public interface IEventImageDomainRepository : IDomainRepository<EventImage>
{
    Task Delete(Guid id, CancellationToken cancellationToken = default);
}

public interface ISaleDomainRepository : IDomainRepository<Sale>
{
    Task<Sale?> Find(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>EVT-R8: the tickets on Confirmed or Pending payment Orders for these Performances.</summary>
    Task<SalesFigures> FiguresFor(IReadOnlyCollection<Guid> performanceIds, CancellationToken cancellationToken = default);
}

public interface ITeamMemberDomainRepository : IDomainRepository<TeamMember>
{
    Task<TeamMember?> Find(Guid organizerId, Guid accountId, CancellationToken cancellationToken = default);

    Task Delete(TeamMember member, CancellationToken cancellationToken = default);
}
