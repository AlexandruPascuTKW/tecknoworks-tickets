using Sales.Application.Common;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands.ExpireLapsedOrders;

/// <summary>
/// SAL-R5 — Orders whose Hold ran out become Expired. Sent by a background job; it only catches the
/// stored status up, because Availability already ignores a lapsed Hold (ADR-003).
/// </summary>
public class ExpireLapsedOrdersCommand : IRequest<Result>
{
    public class ExpireLapsedOrdersCommandHandler : IRequestHandler<ExpireLapsedOrdersCommand, Result>
    {
        private readonly IOrderDomainRepository orders;
        private readonly SalesClock clock;

        public ExpireLapsedOrdersCommandHandler(IOrderDomainRepository orders, SalesClock clock)
        {
            this.orders = orders;
            this.clock = clock;
        }

        public async Task<Result> Handle(ExpireLapsedOrdersCommand request, CancellationToken cancellationToken)
        {
            foreach (var order in await orders.Lapsed(clock.UtcNow, cancellationToken))
            {
                order.Expire();
                await orders.Save(order, cancellationToken);
            }

            return Result.Success;
        }
    }
}
