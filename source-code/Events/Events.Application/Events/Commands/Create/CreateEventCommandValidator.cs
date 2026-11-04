using Events.Application.Events.Commands.Common;
using Events.Domain.Models.Events;

namespace Events.Application.Events.Commands.Create;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(c => c.OrganizerId).NotEmpty();
        RuleFor(c => c.VenueId).NotEmpty().WithMessage("Venue is required.");

        RuleFor(c => c.EventName)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(EventModelConstants.Event.MaxNameLength);

        RuleFor(c => c.Description).MaximumLength(EventModelConstants.Event.MaxDescriptionLength);

        RuleFor(c => c.Image).MustBeAcceptableImage();
    }
}
