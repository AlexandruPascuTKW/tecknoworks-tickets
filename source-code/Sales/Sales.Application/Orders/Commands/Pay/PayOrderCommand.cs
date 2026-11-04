using Sales.Application.Common;
using Sales.Application.Services;
using Sales.Domain.Models.Orders;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands.Pay;

/// <summary>SAL-A2 — Pay for an Order. Starts a payment and says where the Attendee enters their card.</summary>
public class PayOrderCommand : EntityCommand, IRequest<PayOrderResponse>
{
    public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, PayOrderResponse>
    {
        private readonly IOrderDomainRepository orders;
        private readonly IPaymentProvider provider;
        private readonly ICurrentUser user;
        private readonly SalesClock clock;

        public PayOrderCommandHandler(
            IOrderDomainRepository orders,
            IPaymentProvider provider,
            ICurrentUser user,
            SalesClock clock)
        {
            this.orders = orders;
            this.provider = provider;
            this.user = user;
            this.clock = clock;
        }

        public async Task<PayOrderResponse> Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            // SAL-R1: an Attendee acts only on their own Orders.
            var order = await orders.Find(request.Id, cancellationToken);
            if (order is null || order.Buyer.AccountId != user.UserId)
            {
                throw new NotFoundException(nameof(Order), request.Id);
            }

            // SAL-R12: no Payment for an Order that is Confirmed or Expired.
            order.EnsureCanBePaid(clock.UtcNow);

            // SAL-R12: a Payment is for the Order's full total.
            var hosted = await provider.StartHostedPayment(order.Id, order.OrderTotal.Amount, cancellationToken);

            order.AddPayment(hosted.ProviderReference, clock.UtcNow);

            await orders.Save(order, cancellationToken);

            return new PayOrderResponse(hosted.RedirectUrl);
        }
    }
}

public record PayOrderResponse(string RedirectUrl);
