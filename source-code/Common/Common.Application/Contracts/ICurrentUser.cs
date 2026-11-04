/// <summary>The caller of the current request. Everything is null/false for a Visitor.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Name { get; }

    string? Email { get; }

    /// <summary>A platform role carried in the token: PlatformAdmin or FinanceManager (ADR-001).</summary>
    bool IsInRole(string role);
}
