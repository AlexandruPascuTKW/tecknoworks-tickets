using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sales.Domain.Models.Orders;

namespace Sales.Infrastructure.Configurations;

/// <summary>ADR-006: money as whole cents, and instants always read back as UTC.</summary>
internal static class Converters
{
    public static readonly ValueConverter<Money, long> Cents = new(
        money => decimal.ToInt64(money.Amount * 100m),
        cents => new Money(cents / 100m));

    public static readonly ValueConverter<DateTime, DateTime> Utc = new(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
