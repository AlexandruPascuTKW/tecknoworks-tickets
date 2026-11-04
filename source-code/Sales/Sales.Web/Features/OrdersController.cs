using Sales.Application.Orders;
using Sales.Application.Orders.Commands.Pay;
using Sales.Application.Orders.Commands.StartCheckout;
using Sales.Application.Orders.Queries.Checkout;

namespace Sales.Web.Features;

/// <summary>An Attendee buying: checkout, the Order in checkout and paying (SAL-A1, SAL-V1, SAL-A2).</summary>
[Authorize]
public class OrdersController : ApiController
{
    /// <summary>SAL-A1 — Start checkout.</summary>
    [HttpPost]
    public async Task<ActionResult<StartCheckoutResponse>> StartCheckout(StartCheckoutCommand command)
        => await Send(command);

    /// <summary>SAL-V1 — See an Order in checkout.</summary>
    [HttpGet(Id)]
    public async Task<ActionResult<OrderCheckoutResponse>> Checkout([FromRoute] OrderCheckoutQuery query)
        => await Send(query);

    /// <summary>SAL-A2 — Pay for an Order.</summary>
    [HttpPost(Id)]
    public async Task<ActionResult<PayOrderResponse>> Pay([FromRoute] PayOrderCommand command)
        => await Send(command);
}
