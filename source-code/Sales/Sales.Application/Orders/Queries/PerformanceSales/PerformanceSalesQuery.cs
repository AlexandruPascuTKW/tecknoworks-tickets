namespace Sales.Application.Orders.Queries.PerformanceSales;

/// <summary>SAL-V3 — See a Performance's sales. Only the Finance manager (SAL-R1).</summary>
public class PerformanceSalesQuery : EntityCommand, IRequest<PerformanceSalesResponse>
{
    public class PerformanceSalesQueryHandler : IRequestHandler<PerformanceSalesQuery, PerformanceSalesResponse>
    {
        private readonly IOrderQueryRepository orders;
        private readonly ICurrentUser user;

        public PerformanceSalesQueryHandler(IOrderQueryRepository orders, ICurrentUser user)
        {
            this.orders = orders;
            this.user = user;
        }

        public async Task<PerformanceSalesResponse> Handle(PerformanceSalesQuery request, CancellationToken cancellationToken)
        {
            if (!user.IsInRole("FinanceManager"))
            {
                throw new ForbiddenException("Only the finance manager can see a performance's sales.");
            }

            return await orders.PerformanceSales(request.Id, cancellationToken)
                   ?? throw new NotFoundException("Performance", request.Id);
        }
    }
}
