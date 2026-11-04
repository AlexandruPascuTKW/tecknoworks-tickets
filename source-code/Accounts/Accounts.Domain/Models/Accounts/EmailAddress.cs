using System.Text.RegularExpressions;
using Accounts.Domain.Exceptions;

namespace Accounts.Domain.Models.Accounts;

/// <summary>ACC-F2 — a valid email address, kept lower-case. One Account per address (ACC-R2).</summary>
public class EmailAddress : ValueObject
{
    private static readonly Regex Shape = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public EmailAddress(string value)
    {
        value = (value ?? string.Empty).Trim().ToLowerInvariant();

        if (!Shape.IsMatch(value) || value.Length > AccountModelConstants.Account.MaxEmailLength)
        {
            throw new InvalidAccountException("Enter a valid email address.");
        }

        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
