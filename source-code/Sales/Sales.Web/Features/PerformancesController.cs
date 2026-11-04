using Sales.Application.Orders;
using Sales.Application.Orders.Queries.PerformanceSales;
using Sales.Application.Performances;
using Sales.Application.Performances.Queries.Availability;
using Sales.Application.Performances.Queries.GetAll;

namespace Sales.Web.Features;

[Authorize]
public class PerformancesController : ApiController
{
    /// <summary>SAL-V2 — See what's left for the Performances of an Event. Open to everyone.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<List<PerformanceAvailabilityResponse>>> Availability(
        [FromQuery] PerformanceAvailabilityQuery query)
        => await Send(query);

    /// <summary>The Performances the Finance manager picks from.</summary>
    [HttpGet, Authorize(Roles = "FinanceManager")]
    public async Task<ActionResult<List<PerformanceListItem>>> All()
        => await Send(new GetAllPerformancesQuery());

    /// <summary>SAL-V3 — See a Performance's sales (Finance manager).</summary>
    [HttpGet(Id), Authorize(Roles = "FinanceManager")]
    public async Task<ActionResult<PerformanceSalesResponse>> Sales([FromRoute] PerformanceSalesQuery query)
        => await Send(query);
}
