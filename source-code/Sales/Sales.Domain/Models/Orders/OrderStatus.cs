namespace Sales.Domain.Models.Orders;

/// <summary>SAL-F4 — Pending payment, Confirmed or Expired.</summary>
public class OrderStatus : Enumeration
{
    public static readonly OrderStatus PendingPayment = new(1, "Pending payment");
    public static readonly OrderStatus Confirmed = new(2, "Confirmed");
    public static readonly OrderStatus Expired = new(3, "Expired");

    private OrderStatus(int value, string name) : base(value, name)
    {
    }
}
