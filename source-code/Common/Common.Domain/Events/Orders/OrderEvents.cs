public sealed record OrderTicketLine(Guid TicketTypeId, string TicketTypeName, int NumberOfTickets);

/// <summary>Raised by Sales when checkout starts (SAL-A1).</summary>
public sealed record OrderPlacedEvent(Guid OrderId, Guid PerformanceId, OrderTicketLine[] Lines) : IDomainEvent;

/// <summary>Raised by Sales when an Order becomes Confirmed, on time or late (SAL-R12, SAL-R13).</summary>
public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid PerformanceId,
    Guid OrganizerId,
    Guid BuyerAccountId,
    string BuyerName,
    OrderTicketLine[] Lines) : IDomainEvent;

/// <summary>Raised by Sales when an Order's Hold runs out or is replaced (SAL-R5, SAL-R15).</summary>
public sealed record OrderExpiredEvent(Guid OrderId, Guid PerformanceId) : IDomainEvent;
