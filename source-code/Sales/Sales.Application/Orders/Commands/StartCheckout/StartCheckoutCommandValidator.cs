using Sales.Domain.Models.Orders;

namespace Sales.Application.Orders.Commands.StartCheckout;

public class StartCheckoutCommandValidator : AbstractValidator<StartCheckoutCommand>
{
    private static readonly string TooMany =
        $"You can buy up to {OrderModelConstants.Order.MaxTicketsPerOrder} tickets in one order.";

    public StartCheckoutCommandValidator()
    {
        RuleFor(c => c.PerformanceId).NotEmpty().WithMessage("Performance is required.");

        RuleFor(c => c.Lines).NotEmpty().WithMessage("Choose at least one ticket.");

        // SAL-F10 / SAL-R4: 1 to 8 on a line, and 8 across the whole Order.
        RuleForEach(c => c.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.TicketTypeId).NotEmpty();
            line.RuleFor(l => l.NumberOfTickets)
                .InclusiveBetween(1, OrderModelConstants.Order.MaxTicketsPerOrder)
                .WithMessage(TooMany);
        });

        RuleFor(c => c.Lines.Sum(l => l.NumberOfTickets))
            .LessThanOrEqualTo(OrderModelConstants.Order.MaxTicketsPerOrder)
            .WithMessage(TooMany)
            .OverridePropertyName(nameof(StartCheckoutCommand.Lines));
    }
}
