namespace Accounts.Application.Services;

/// <summary>ACC-R3 / ADR-007: delivers the link where a person chooses their own Password.</summary>
public interface IInvitationSender
{
    Task Send(string email, string name, string link, CancellationToken cancellationToken = default);
}
