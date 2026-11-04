using Admission.Domain.Models.Tickets;

namespace Admission.Application.Tickets;

/// <summary>ADM-V1 — one of an Attendee's Tickets, with where and when it gets the person in.</summary>
public record MyTicketItem(
    Guid Id,
    string TicketCode,
    string TicketTypeName,
    Guid PerformanceId,
    string EventName,
    string VenueName,
    DateOnly Date,
    TimeOnly DoorsTime,
    TimeOnly StartTime);

public record DoorListItem(string TicketCode, string TicketTypeName, string BuyersName);

/// <summary>ADM-V2 — every Ticket for one Performance, ready to print.</summary>
public record DoorListResponse(
    Guid PerformanceId,
    Guid OrganizerId,
    string EventName,
    DateOnly Date,
    TimeOnly StartTime,
    List<DoorListItem> Tickets);

public interface ITicketQueryRepository : IQueryRepository<Ticket>
{
    Task<List<MyTicketItem>> Mine(Guid buyerAccountId, CancellationToken cancellationToken = default);

    Task<DoorListResponse?> DoorList(Guid performanceId, CancellationToken cancellationToken = default);
}
