namespace Sales.Domain.Models.Orders;

/// <summary>An attempt to pay an Order's total by card through the Payment provider (SAL glossary).</summary>
public class Payment : Entity
{
    internal Payment(string providerReference)
    {
        ProviderReference = providerReference;
    }

    private Payment()
    {
        ProviderReference = default!;
    }

    /// <summary>The provider's own reference for this payment. Never a card number (SAL-R11).</summary>
    public string ProviderReference { get; private set; }

    /// <summary>SAL-F12: empty until the Payment provider answers.</summary>
    public PaymentResult? Result { get; private set; }

    internal void Record(PaymentResult result) => Result = result;
}
