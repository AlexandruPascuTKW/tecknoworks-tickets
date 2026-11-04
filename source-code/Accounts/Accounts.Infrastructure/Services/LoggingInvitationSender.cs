using Accounts.Application.Services;
using Microsoft.Extensions.Logging;

namespace Accounts.Infrastructure.Services;

/// <summary>
/// ADR-007: there is no mail server yet, so the invitation link is written to the log. Replace with a
/// real sender before Organizers are onboarded for real; never ship this to production.
/// </summary>
internal class LoggingInvitationSender : IInvitationSender
{
    private readonly ILogger<LoggingInvitationSender> logger;

    public LoggingInvitationSender(ILogger<LoggingInvitationSender> logger) => this.logger = logger;

    public Task Send(string email, string name, string link, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("INVITATION for {Name} <{Email}>: choose a password at {Link}", name, email, link);
        return Task.CompletedTask;
    }
}
