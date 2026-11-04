using Accounts.Application.Organizers;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Accounts.Queries.Me;

/// <summary>The signed-in person, their platform role and the Teams they are on — what the UI needs to navigate by role.</summary>
public class CurrentAccountQuery : IRequest<CurrentAccountResponse>
{
    public class CurrentAccountQueryHandler : IRequestHandler<CurrentAccountQuery, CurrentAccountResponse>
    {
        private readonly IAccountDomainRepository accounts;
        private readonly IOrganizerQueryRepository organizers;
        private readonly ICurrentUser user;

        public CurrentAccountQueryHandler(
            IAccountDomainRepository accounts,
            IOrganizerQueryRepository organizers,
            ICurrentUser user)
        {
            this.accounts = accounts;
            this.organizers = organizers;
            this.user = user;
        }

        public async Task<CurrentAccountResponse> Handle(CurrentAccountQuery request, CancellationToken cancellationToken)
        {
            // ACC-R1: a person sees only their own Account.
            var id = user.UserId ?? throw new ForbiddenException("Sign in first.");

            var account = await accounts.Find(id, cancellationToken)
                          ?? throw new NotFoundException("Account", id);

            return new CurrentAccountResponse(
                account.Id,
                account.Name,
                account.Email,
                account.PlatformRole?.Name,
                await organizers.TeamsOf(id, cancellationToken));
        }
    }
}
