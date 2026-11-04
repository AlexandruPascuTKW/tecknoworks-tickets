using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Models.Organizers;

namespace Accounts.Application.Organizers.Commands.Onboard;

public class OnboardOrganizerCommandValidator : AbstractValidator<OnboardOrganizerCommand>
{
    public OnboardOrganizerCommandValidator()
    {
        RuleFor(c => c.OrganizerName)
            .NotEmpty().WithMessage("Organizer name is required.")
            .MaximumLength(OrganizerModelConstants.Organizer.MaxNameLength);

        RuleFor(c => c.Name).MaximumLength(AccountModelConstants.Account.MaxNameLength);

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");
    }
}
