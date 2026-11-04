using Events.Domain.Exceptions;

namespace Events.Domain.Models.Events;

/// <summary>EVT-F13 — an amount in euros, above zero, to two decimal places (ADR-006).</summary>
public class Price : ValueObject
{
    public Price(decimal amount)
    {
        if (amount <= 0 || decimal.Round(amount, EventModelConstants.TicketType.PriceDecimals) != amount)
        {
            throw new InvalidEventException("Price must be above zero, with at most two decimals.");
        }

        Amount = amount;
    }

    public decimal Amount { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}
