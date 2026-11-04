using Accounts.Domain.Models.Accounts;

namespace Accounts.Application.Accounts.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(AccountModelConstants.Account.MaxNameLength);

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.")
            .MaximumLength(AccountModelConstants.Account.MaxEmailLength);

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(AccountModelConstants.Account.MinPasswordLength, AccountModelConstants.Account.MaxPasswordLength)
            .WithMessage($"Password must have {AccountModelConstants.Account.MinPasswordLength} to {AccountModelConstants.Account.MaxPasswordLength} characters.");
    }
}
