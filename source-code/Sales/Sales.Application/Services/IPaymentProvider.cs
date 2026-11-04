namespace Sales.Application.Services;

/// <summary>Where to send the Attendee to pay, and the provider's own reference for that payment.</summary>
public record HostedPayment(string ProviderReference, string RedirectUrl);

/// <summary>The provider telling us how a payment ended. The signature proves it came from the provider.</summary>
public record PaymentNotification(string ProviderReference, string Result, string Signature);

/// <summary>
/// ADR-004 / SAL-R11: card payment through a hosted page. Card details only ever reach the provider's own
/// page; nothing here takes a card number.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>Starts a payment for exactly this amount (SAL-R12) and says where to send the Attendee.</summary>
    Task<HostedPayment> StartHostedPayment(Guid orderId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>SAL-R13: gives the whole payment back.</summary>
    Task Refund(string providerReference, CancellationToken cancellationToken = default);

    /// <summary>Whether a notification really came from the provider.</summary>
    bool Verify(PaymentNotification notification);
}

public record SimulatedPaymentInfo(string ProviderReference, Guid OrderId, decimal Amount, string Status);

/// <summary>
/// ADR-004: the simulated provider's own "hosted page", used until a real provider is chosen. Development
/// only; a real provider has its own page and none of this.
/// </summary>
public interface ISimulatedPaymentPages
{
    SimulatedPaymentInfo? Find(string providerReference);

    /// <summary>Ends the payment as the Attendee chose and returns the signed notification the provider would send.</summary>
    PaymentNotification Complete(string providerReference, bool approve);
}
