using Events.Domain.Models.Events;

namespace Events.Application.Events.Commands.ChangeTicketType;

public class ChangeTicketTypeCommandValidator : AbstractValidator<ChangeTicketTypeCommand>
{
    public ChangeTicketTypeCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
        RuleFor(c => c.TicketTypeId).NotEmpty();

        RuleFor(c => c.TicketTypeName)
            .NotEmpty().WithMessage("Ticket type name is required.")
            .MaximumLength(EventModelConstants.TicketType.MaxNameLength)
            .When(c => c.TicketTypeName is not null);

        RuleFor(c => c.Price!.Value)
            .GreaterThan(0)
            .PrecisionScale(EventModelConstants.TicketType.MaxPriceDigits, EventModelConstants.TicketType.PriceDecimals, false)
            .WithMessage("Price must be above zero, with at most two decimals.")
            .When(c => c.Price is not null);

        RuleFor(c => c.Quantity!.Value)
            .GreaterThan(0).WithMessage("Quantity must be a whole number above zero.")
            .When(c => c.Quantity is not null);
    }
}
