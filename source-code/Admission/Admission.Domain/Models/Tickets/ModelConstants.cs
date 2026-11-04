namespace Admission.Domain.Models.Tickets;

public class TicketModelConstants
{
    public class Ticket
    {
        /// <summary>ADR-009: ten characters, shown as two groups of five.</summary>
        public const int CodeLength = 10;

        /// <summary>ADR-009: digits and capitals without 0, O, 1, I and L, so no two characters look alike.</summary>
        public const string CodeAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    }
}
