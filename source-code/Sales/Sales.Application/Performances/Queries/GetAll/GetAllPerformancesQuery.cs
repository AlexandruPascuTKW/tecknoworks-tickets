namespace Sales.Application.Performances.Queries.GetAll;

/// <summary>The Performances the Finance manager can pick from to see their sales (SAL-V3).</summary>
public class GetAllPerformancesQuery : IRequest<List<PerformanceListItem>>
{
    public class GetAllPerformancesQueryHandler : IRequestHandler<GetAllPerformancesQuery, List<PerformanceListItem>>
    {
        private readonly IPerformanceQueryRepository performances;
        private readonly ICurrentUser user;

        public GetAllPerformancesQueryHandler(IPerformanceQueryRepository performances, ICurrentUser user)
        {
            this.performances = performances;
            this.user = user;
        }

        public async Task<List<PerformanceListItem>> Handle(GetAllPerformancesQuery request, CancellationToken cancellationToken)
        {
            if (!user.IsInRole("FinanceManager"))
            {
                throw new ForbiddenException("Only the finance manager can see every performance's sales.");
            }

            return await performances.ListAll(cancellationToken);
        }
    }
}
