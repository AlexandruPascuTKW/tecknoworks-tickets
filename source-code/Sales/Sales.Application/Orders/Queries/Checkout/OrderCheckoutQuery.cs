using Sales.Application.Common;
using Sales.Domain.Models.Orders;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Queries.Checkout;

/// <summary>
/// SAL-V1 — See an Order in checkout: what is being bought, the Face value, Booking fee and Order total
/// each on their own (SAL-R10), and how long the tickets stay held. Only the Attendee's own Orders (SAL-R1).
/// </summary>
public class OrderCheckoutQuery : EntityCommand, IRequest<OrderCheckoutResponse>
{
    public class OrderCheckoutQueryHandler : IRequestHandler<OrderCheckoutQuery, OrderCheckoutResponse>
    {
        private readonly IOrderDomainRepository orders;
        private readonly ICurrentUser user;
        private readonly SalesClock clock;

        public OrderCheckoutQueryHandler(IOrderDomainRepository orders, ICurrentUser user, SalesClock clock)
        {
            this.orders = orders;
            this.user = user;
            this.clock = clock;
        }

        public async Task<OrderCheckoutResponse> Handle(OrderCheckoutQuery request, CancellationToken cancellationToken)
        {
            var order = await orders.Find(request.Id, cancellationToken);

            if (order is null || order.Buyer.AccountId != user.UserId)
            {
                throw new NotFoundException(nameof(Order), request.Id);
            }

            return new OrderCheckoutResponse(
                order.Id,
                order.PerformanceId,
                order.EventName,
                order.PerformanceStartsAt,
                order.StatusAt(clock.UtcNow).Name, // a lapsed Hold already counts as Expired (ADR-003)
                order.PlacedAt,
                order.HoldEndsAt,
                order.Lines
                    .Select(l => new OrderLineResponse(l.TicketTypeName, l.NumberOfTickets, l.TicketPrice.Amount))
                    .ToList(),
                order.FaceValue.Amount,
                order.BookingFee.Amount,
                order.OrderTotal.Amount,
                order.LatestPayment?.Result?.Name);
        }
    }
}
