namespace Accounts.Application.Accounts.Commands.SignIn;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().WithMessage("Email address is required.");
        RuleFor(c => c.Password).NotEmpty().WithMessage("Password is required.");
    }
}
