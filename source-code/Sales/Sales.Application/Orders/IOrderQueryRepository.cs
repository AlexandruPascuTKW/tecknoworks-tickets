using Sales.Domain.Models.Orders;

namespace Sales.Application.Orders;

public record OrderLineResponse(string TicketTypeName, int NumberOfTickets, decimal TicketPrice);

/// <summary>SAL-V1 — an Order in checkout.</summary>
public record OrderCheckoutResponse(
    Guid Id,
    Guid PerformanceId,
    string EventName,
    DateTime PerformanceStartsAt,
    string OrderStatus,
    DateTime PlacedAt,
    DateTime HoldEndsAt,
    List<OrderLineResponse> Lines,
    decimal FaceValue,
    decimal BookingFee,
    decimal OrderTotal,
    string? PaymentResult);

public record SoldOrderItem(
    Guid OrderId,
    string AttendeeName,
    string AttendeeEmail,
    DateTime PlacedAt,
    string OrderStatus,
    List<OrderLineResponse> Lines,
    decimal FaceValue,
    decimal BookingFee,
    decimal OrderTotal,
    string? PaymentResult);

public record TicketTypeSold(string TicketTypeName, int NumberOfTickets);

/// <summary>SAL-V3 — everything sold for one Performance.</summary>
public record PerformanceSalesResponse(
    Guid PerformanceId,
    string EventName,
    DateTime PerformanceStartsAt,
    List<SoldOrderItem> Orders,
    List<TicketTypeSold> TicketsSold,
    decimal TotalFaceValue,
    decimal TotalBookingFee);

public interface IOrderQueryRepository : IQueryRepository<Order>
{
    /// <summary>SAL-V3: Confirmed Orders, and Expired ones whose Payment was Refunded (SAL-R13).</summary>
    Task<PerformanceSalesResponse?> PerformanceSales(Guid performanceId, CancellationToken cancellationToken = default);
}
