using Accounts.Application.Services;
using Accounts.Domain.Exceptions;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Accounts.Commands.SignIn;

/// <summary>ACC-A2 — Sign in.</summary>
public class SignInCommand : IRequest<SignInResponse>
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public class SignInCommandHandler : IRequestHandler<SignInCommand, SignInResponse>
    {
        private readonly IAccountDomainRepository accounts;
        private readonly IPasswordHasher hasher;
        private readonly ITokenIssuer tokens;

        public SignInCommandHandler(IAccountDomainRepository accounts, IPasswordHasher hasher, ITokenIssuer tokens)
        {
            this.accounts = accounts;
            this.hasher = hasher;
            this.tokens = tokens;
        }

        public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            var account = await accounts.FindByEmail(new EmailAddress(request.Email), cancellationToken);

            // ACC-R3: an Account still waiting on its invitation has no Password to sign in with.
            if (account is null || !account.HasPassword || !hasher.Verify(request.Password, account.PasswordHash!))
            {
                throw new InvalidAccountException("Wrong email address or password.");
            }

            return SignInResponse.For(account, tokens);
        }
    }
}
