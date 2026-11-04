using Accounts.Application.Organizers;

namespace Accounts.Application.Accounts.Queries.Me;

public record CurrentAccountResponse(
    Guid Id,
    string Name,
    string Email,
    string? PlatformRole,
    List<MyTeam> Teams);
