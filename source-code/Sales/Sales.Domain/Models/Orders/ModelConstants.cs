namespace Sales.Domain.Models.Orders;

public class OrderModelConstants
{
    public class Order
    {
        /// <summary>SAL-R4 / SAL-F10: at most eight tickets, counted across all Ticket types.</summary>
        public const int MaxTicketsPerOrder = 8;

        /// <summary>SAL-R5: a Hold lasts ten minutes.</summary>
        public static readonly TimeSpan HoldPeriod = TimeSpan.FromMinutes(10);

        /// <summary>SAL-R8: €1.50 for each ticket.</summary>
        public const decimal BookingFeePerTicket = 1.50m;
    }
}
