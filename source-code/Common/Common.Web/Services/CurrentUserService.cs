using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class CurrentUserService : ICurrentUser
{
    private readonly ClaimsPrincipal? user;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => user = httpContextAccessor.HttpContext?.User;

    public Guid? UserId
        => Guid.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Name => user?.FindFirstValue(ClaimTypes.Name);

    public string? Email => user?.FindFirstValue(ClaimTypes.Email);

    public bool IsInRole(string role) => user?.IsInRole(role) ?? false;
}
