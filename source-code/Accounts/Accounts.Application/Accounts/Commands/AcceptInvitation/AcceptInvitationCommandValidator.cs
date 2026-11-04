using Accounts.Domain.Models.Accounts;

namespace Accounts.Application.Accounts.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(c => c.Token).NotEmpty().WithMessage("The invitation link is incomplete.");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(AccountModelConstants.Account.MinPasswordLength, AccountModelConstants.Account.MaxPasswordLength)
            .WithMessage($"Password must have {AccountModelConstants.Account.MinPasswordLength} to {AccountModelConstants.Account.MaxPasswordLength} characters.");
    }
}
