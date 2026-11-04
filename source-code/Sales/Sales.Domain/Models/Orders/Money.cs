namespace Sales.Domain.Models.Orders;

/// <summary>An amount in euros to two decimal places (SAL-F6 to SAL-F8; stored as cents, ADR-006).</summary>
public class Money : ValueObject
{
    public static readonly Money Zero = new(0m);

    public Money(decimal amount) => Amount = decimal.Round(amount, 2);

    public decimal Amount { get; }

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public Money Times(int count) => new(Amount * count);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}
