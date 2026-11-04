using Sales.Application.Orders.Commands.RecordPaymentResult;

namespace Sales.Web.Features;

/// <summary>Where the Payment provider reports how a payment ended (SAL-R12, SAL-R13).</summary>
[AllowAnonymous]
public class PaymentsController : ApiController
{
    /// <summary>The provider's callback. It is checked to be the provider's before anything changes.</summary>
    [HttpPost]
    public async Task<ActionResult> Callback(RecordPaymentResultCommand command)
        => await Send(command);
}
