namespace Events.Domain.Models.Events;

public class EventModelConstants
{
    public class Event
    {
        public const int MaxNameLength = 150;
        public const int MaxDescriptionLength = 4000;
    }

    public class TicketType
    {
        public const int MaxNameLength = 60;

        // EVT-F13: euros, above zero, two decimals.
        public const int MaxPriceDigits = 10;
        public const int PriceDecimals = 2;
    }
}
