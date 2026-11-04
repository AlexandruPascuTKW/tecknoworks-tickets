namespace Sales.Domain.Models.Performances;

/// <summary>A Ticket type as Events publishes it, to build or refresh an <see cref="OfferedTicketType"/>.</summary>
public record OfferedTicketTypeData(Guid TicketTypeId, string Name, decimal Price, int Quantity, bool OnSale);
