using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Models.Organizers;

namespace Accounts.Application.Organizers.Commands.AddTeamMember;

public class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(c => c.OrganizerId).NotEmpty();

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        // ACC-F5
        RuleFor(c => c.TeamRole)
            .Must(r => Enumeration.GetAll<TeamRole>().Any(t => t.Name == r))
            .WithMessage("Team role must be Owner or Manager.");

        RuleFor(c => c.Name).MaximumLength(AccountModelConstants.Account.MaxNameLength);
    }
}
