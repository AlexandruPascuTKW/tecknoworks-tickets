using System.Security.Cryptography;

namespace Admission.Domain.Models.Tickets;

/// <summary>
/// ADM-F1, ADM-R3, ADR-009 — the code that identifies one Ticket at the door. Ten random characters from an
/// unambiguous alphabet, drawn with the cryptographic random generator so it cannot be guessed.
/// </summary>
public class TicketCode : ValueObject
{
    public TicketCode(string value)
    {
        value = (value ?? string.Empty).Replace("-", string.Empty).Trim().ToUpperInvariant();

        if (value.Length != TicketModelConstants.Ticket.CodeLength
            || value.Any(c => !TicketModelConstants.Ticket.CodeAlphabet.Contains(c)))
        {
            throw new ArgumentException("Not a ticket code.", nameof(value));
        }

        Value = value;
    }

    /// <summary>As stored: ten characters, no hyphen.</summary>
    public string Value { get; }

    /// <summary>As shown and printed: two groups of five, K7Q2M-9XMF3.</summary>
    public string Display => $"{Value[..5]}-{Value[5..]}";

    public static TicketCode New()
    {
        var alphabet = TicketModelConstants.Ticket.CodeAlphabet;
        var characters = new char[TicketModelConstants.Ticket.CodeLength];

        for (var i = 0; i < characters.Length; i++)
        {
            characters[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        }

        return new TicketCode(new string(characters));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
