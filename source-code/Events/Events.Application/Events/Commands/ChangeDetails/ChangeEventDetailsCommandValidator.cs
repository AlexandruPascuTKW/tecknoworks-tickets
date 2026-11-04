using Events.Application.Events.Commands.Common;
using Events.Domain.Models.Events;

namespace Events.Application.Events.Commands.ChangeDetails;

public class ChangeEventDetailsCommandValidator : AbstractValidator<ChangeEventDetailsCommand>
{
    public ChangeEventDetailsCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();

        RuleFor(c => c.EventName)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(EventModelConstants.Event.MaxNameLength)
            .When(c => c.EventName is not null);

        RuleFor(c => c.Description).MaximumLength(EventModelConstants.Event.MaxDescriptionLength);

        RuleFor(c => c.Image).MustBeAcceptableImage();
    }
}
