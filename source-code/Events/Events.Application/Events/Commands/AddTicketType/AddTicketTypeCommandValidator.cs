using Events.Domain.Models.Events;

namespace Events.Application.Events.Commands.AddTicketType;

public class AddTicketTypeCommandValidator : AbstractValidator<AddTicketTypeCommand>
{
    public AddTicketTypeCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();

        RuleFor(c => c.TicketTypeName)
            .NotEmpty().WithMessage("Ticket type name is required.")
            .MaximumLength(EventModelConstants.TicketType.MaxNameLength);

        // EVT-F13
        RuleFor(c => c.Price)
            .GreaterThan(0)
            .PrecisionScale(EventModelConstants.TicketType.MaxPriceDigits, EventModelConstants.TicketType.PriceDecimals, false)
            .WithMessage("Price must be above zero, with at most two decimals.");

        // EVT-F14
        RuleFor(c => c.Quantity).GreaterThan(0).WithMessage("Quantity must be a whole number above zero.");
    }
}
