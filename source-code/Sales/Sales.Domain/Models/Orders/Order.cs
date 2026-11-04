using Sales.Domain.Exceptions;

namespace Sales.Domain.Models.Orders;

/// <summary>
/// One Attendee's purchase of tickets for one Performance (SAL-R2, R4, R5, R7 to R10, R12, R13, R15).
/// Whether a Hold has run out is decided by comparing <see cref="HoldEndsAt"/> with now, wherever it
/// matters; a background sweep only catches the stored status up (ADR-003).
/// </summary>
public class Order : Entity, IAggregateRoot
{
    private readonly List<OrderLine> lines = new();
    private readonly List<Payment> payments = new();

    internal Order(
        Buyer buyer,
        Guid performanceId,
        Guid organizerId,
        string eventName,
        DateTime performanceStartsAt,
        DateTime placedAtUtc,
        IEnumerable<OrderLine> orderLines)
    {
        lines.AddRange(orderLines);

        var tickets = lines.Sum(l => l.NumberOfTickets);

        // SAL-R4 / SAL-F10: at least one ticket, at most eight, across all Ticket types.
        if (tickets < 1 || tickets > OrderModelConstants.Order.MaxTicketsPerOrder || lines.Any(l => l.NumberOfTickets < 1))
        {
            throw new CheckoutRefusedException(
                $"You can buy up to {OrderModelConstants.Order.MaxTicketsPerOrder} tickets in one order.");
        }

        Buyer = buyer;
        PerformanceId = performanceId;
        OrganizerId = organizerId;
        EventName = eventName;
        PerformanceStartsAt = performanceStartsAt;
        Status = OrderStatus.PendingPayment; // SAL-R2
        PlacedAt = placedAtUtc;
        HoldEndsAt = placedAtUtc + OrderModelConstants.Order.HoldPeriod; // SAL-R5

        // SAL-R8 / SAL-R9: fixed once, when the Order is placed.
        FaceValue = lines.Aggregate(Money.Zero, (sum, line) => sum + line.TicketPrice.Times(line.NumberOfTickets));
        BookingFee = new Money(OrderModelConstants.Order.BookingFeePerTicket * tickets);
        OrderTotal = FaceValue + BookingFee;

        RaiseEvent(() => new OrderPlacedEvent(Id, PerformanceId, TicketLines()));
    }

    private Order()
    {
        Buyer = default!;
        EventName = default!;
        Status = default!;
        FaceValue = default!;
        BookingFee = default!;
        OrderTotal = default!;
    }

    public Buyer Buyer { get; private set; }

    /// <summary>The Performance's id as Events publishes it — a plain Guid across the context boundary.</summary>
    public Guid PerformanceId { get; private set; }

    public Guid OrganizerId { get; private set; }

    public string EventName { get; private set; }

    /// <summary>Local date and time at the Venue, for showing.</summary>
    public DateTime PerformanceStartsAt { get; private set; }

    public OrderStatus Status { get; private set; }

    /// <summary>SAL-F3 (UTC).</summary>
    public DateTime PlacedAt { get; private set; }

    /// <summary>SAL-F5: Placed at plus the Hold period (UTC).</summary>
    public DateTime HoldEndsAt { get; private set; }

    public Money FaceValue { get; private set; }

    public Money BookingFee { get; private set; }

    public Money OrderTotal { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => lines.AsReadOnly();

    public IReadOnlyCollection<Payment> Payments => payments.AsReadOnly();

    public int NumberOfTickets => lines.Sum(l => l.NumberOfTickets);

    /// <summary>SAL-R5: the Hold has run out and the Order was not Confirmed.</summary>
    public bool HasLapsed(DateTime nowUtc) => Status == OrderStatus.PendingPayment && nowUtc >= HoldEndsAt;

    /// <summary>What the Order is, as of now — a lapsed Pending payment Order already counts as Expired.</summary>
    public OrderStatus StatusAt(DateTime nowUtc) => HasLapsed(nowUtc) ? OrderStatus.Expired : Status;

    public Payment? LatestPayment => payments.LastOrDefault();

    /// <summary>SAL-R12: no Payment is started for an Order that is Confirmed or Expired.</summary>
    public Order EnsureCanBePaid(DateTime nowUtc)
    {
        if (Status != OrderStatus.PendingPayment || HasLapsed(nowUtc))
        {
            throw new InvalidOrderException("This order can no longer be paid.");
        }

        return this;
    }

    public Order AddPayment(string providerReference, DateTime nowUtc)
    {
        EnsureCanBePaid(nowUtc);

        payments.Add(new Payment(providerReference));
        return this;
    }

    public Payment? PaymentFor(string providerReference)
        => payments.FirstOrDefault(p => p.ProviderReference == providerReference);

    /// <summary>SAL-R12: a Declined Payment leaves the Order Pending payment, to be paid again.</summary>
    public Order RecordDeclined(string providerReference)
    {
        FindPayment(providerReference).Record(PaymentResult.Declined);
        return this;
    }

    /// <summary>SAL-R12: Paid before the Hold ends makes the Order Confirmed.</summary>
    public Order ConfirmOnTime(string providerReference, DateTime nowUtc)
    {
        if (Status != OrderStatus.PendingPayment || HasLapsed(nowUtc))
        {
            throw new InvalidOrderException("This order's hold has ended.");
        }

        FindPayment(providerReference).Record(PaymentResult.Paid);
        Confirm();
        return this;
    }

    /// <summary>SAL-R13: Paid after the Hold ran out, and the same tickets are still free.</summary>
    public Order ConfirmLate(string providerReference)
    {
        FindPayment(providerReference).Record(PaymentResult.Paid);
        Confirm();
        return this;
    }

    /// <summary>SAL-R13: Paid after the Hold ran out, and the tickets have gone — the whole Payment is refunded.</summary>
    public Order MarkRefunded(string providerReference)
    {
        FindPayment(providerReference).Record(PaymentResult.Refunded);
        Expire();
        return this;
    }

    /// <summary>SAL-R5 / SAL-R15: the Hold ran out, or a newer checkout replaced this one.</summary>
    public Order Expire()
    {
        if (Status != OrderStatus.PendingPayment)
        {
            return this;
        }

        Status = OrderStatus.Expired;
        RaiseEvent(() => new OrderExpiredEvent(Id, PerformanceId));
        return this;
    }

    private void Confirm()
    {
        Status = OrderStatus.Confirmed;
        RaiseEvent(() => new OrderConfirmedEvent(
            Id,
            PerformanceId,
            OrganizerId,
            Buyer.AccountId,
            Buyer.Name,
            TicketLines()));
    }

    private Payment FindPayment(string providerReference)
        => PaymentFor(providerReference)
           ?? throw new InvalidOrderException("This payment does not belong to the order.");

    private OrderTicketLine[] TicketLines()
        => lines.Select(l => new OrderTicketLine(l.TicketTypeId, l.TicketTypeName, l.NumberOfTickets)).ToArray();
}
