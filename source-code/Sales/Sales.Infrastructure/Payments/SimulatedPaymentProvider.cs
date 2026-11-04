using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Sales.Application.Services;
using Sales.Application.Settings;
using Sales.Domain.Exceptions;

namespace Sales.Infrastructure.Payments;

/// <summary>
/// ADR-004 — stands in for the Payment provider until a real one is chosen. Its "hosted page" is a page of
/// the web app that offers an approving and a declining test card, and it can be left open past the Hold.
/// State is in memory, which is fine for a simulator: a restart forgets open payments. Nothing here ever
/// sees a card number (SAL-R11).
/// </summary>
internal class SimulatedPaymentProvider : IPaymentProvider, ISimulatedPaymentPages
{
    private readonly ConcurrentDictionary<string, Entry> payments = new();
    private readonly byte[] key;

    public SimulatedPaymentProvider(IOptions<SalesSettings> settings)
        => key = Encoding.UTF8.GetBytes(settings.Value.SimulatedProviderKey);

    public Task<HostedPayment> StartHostedPayment(Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        var reference = $"sim_{Guid.NewGuid():N}";
        payments[reference] = new Entry(reference, orderId, amount);

        // The simulated page is a route of the web app itself.
        return Task.FromResult(new HostedPayment(reference, $"/simulated-payment/{reference}"));
    }

    public Task Refund(string providerReference, CancellationToken cancellationToken = default)
    {
        if (payments.TryGetValue(providerReference, out var entry))
        {
            lock (entry)
            {
                entry.Status = "Refunded";
            }
        }

        return Task.CompletedTask;
    }

    public bool Verify(PaymentNotification notification)
        => CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Sign(notification.ProviderReference, notification.Result)),
            Encoding.UTF8.GetBytes(notification.Signature ?? string.Empty));

    public SimulatedPaymentInfo? Find(string providerReference)
        => payments.TryGetValue(providerReference, out var entry)
            ? new SimulatedPaymentInfo(entry.Reference, entry.OrderId, entry.Amount, entry.Status)
            : null;

    public PaymentNotification Complete(string providerReference, bool approve)
    {
        var entry = payments.GetValueOrDefault(providerReference)
                    ?? throw new InvalidOrderException("This payment does not exist.");

        lock (entry)
        {
            if (entry.Status != "Open")
            {
                throw new InvalidOrderException("This payment has already been completed.");
            }

            entry.Status = approve ? "Paid" : "Declined";
        }

        var result = approve ? "Paid" : "Declined";

        return new PaymentNotification(providerReference, result, Sign(providerReference, result));
    }

    private string Sign(string providerReference, string result)
        => Convert.ToHexString(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes($"{providerReference}:{result}")));

    private sealed class Entry
    {
        public Entry(string reference, Guid orderId, decimal amount)
        {
            Reference = reference;
            OrderId = orderId;
            Amount = amount;
        }

        public string Reference { get; }
        public Guid OrderId { get; }
        public decimal Amount { get; }
        public string Status { get; set; } = "Open";
    }
}
