namespace Sales.Domain.Models.Orders;

/// <summary>SAL-F12 — Paid, Declined or Refunded.</summary>
public class PaymentResult : Enumeration
{
    public static readonly PaymentResult Paid = new(1, nameof(Paid));
    public static readonly PaymentResult Declined = new(2, nameof(Declined));
    public static readonly PaymentResult Refunded = new(3, nameof(Refunded));

    private PaymentResult(int value, string name) : base(value, name)
    {
    }
}
