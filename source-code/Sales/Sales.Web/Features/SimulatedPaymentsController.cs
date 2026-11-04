using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sales.Application.Orders.Commands.SimulatedPayment;
using Sales.Application.Services;

namespace Sales.Web.Features;

/// <summary>
/// ADR-004 — the simulated Payment provider's hosted page, used until a real provider is chosen. It exists
/// in Development only: anywhere else these routes are simply not there.
/// </summary>
[AllowAnonymous]
public class SimulatedPaymentsController : ApiController
{
    private readonly IWebHostEnvironment environment;

    public SimulatedPaymentsController(IWebHostEnvironment environment) => this.environment = environment;

    [HttpGet("{providerReference}")]
    public async Task<ActionResult<SimulatedPaymentInfo>> Details([FromRoute] SimulatedPaymentQuery query)
        => environment.IsDevelopment() ? await Send(query) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SimulatedPaymentInfo>> Complete(CompleteSimulatedPaymentCommand command)
        => environment.IsDevelopment() ? await Send(command) : NotFound();
}
