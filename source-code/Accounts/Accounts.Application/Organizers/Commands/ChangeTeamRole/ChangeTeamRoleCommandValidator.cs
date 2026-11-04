using Accounts.Domain.Models.Organizers;

namespace Accounts.Application.Organizers.Commands.ChangeTeamRole;

public class ChangeTeamRoleCommandValidator : AbstractValidator<ChangeTeamRoleCommand>
{
    public ChangeTeamRoleCommandValidator()
    {
        RuleFor(c => c.OrganizerId).NotEmpty();
        RuleFor(c => c.AccountId).NotEmpty();

        RuleFor(c => c.TeamRole)
            .Must(r => Enumeration.GetAll<TeamRole>().Any(t => t.Name == r))
            .WithMessage("Team role must be Owner or Manager.");
    }
}
