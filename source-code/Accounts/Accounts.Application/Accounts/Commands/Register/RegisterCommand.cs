using Accounts.Application.Accounts.Commands.SignIn;
using Accounts.Application.Services;
using Accounts.Domain.Exceptions;
using Accounts.Domain.Factories;
using Accounts.Domain.Models.Accounts;
using Accounts.Domain.Repositories;

namespace Accounts.Application.Accounts.Commands.Register;

/// <summary>ACC-A1 — Register. The new Account is signed in straight away.</summary>
public class RegisterCommand : IRequest<SignInResponse>
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, SignInResponse>
    {
        private readonly IAccountDomainRepository accounts;
        private readonly IAccountFactory factory;
        private readonly IPasswordHasher hasher;
        private readonly ITokenIssuer tokens;

        public RegisterCommandHandler(
            IAccountDomainRepository accounts,
            IAccountFactory factory,
            IPasswordHasher hasher,
            ITokenIssuer tokens)
        {
            this.accounts = accounts;
            this.factory = factory;
            this.hasher = hasher;
            this.tokens = tokens;
        }

        public async Task<SignInResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var email = new EmailAddress(request.Email);

            // ACC-R2: one Account per Email address.
            if (await accounts.FindByEmail(email, cancellationToken) is not null)
            {
                throw new InvalidAccountException("An account already uses this email address.");
            }

            var account = factory
                .WithName(request.Name)
                .WithEmail(email.Value)
                .WithPasswordHash(hasher.Hash(request.Password))
                .Build();

            await accounts.Save(account, cancellationToken);

            return SignInResponse.For(account, tokens);
        }
    }
}
