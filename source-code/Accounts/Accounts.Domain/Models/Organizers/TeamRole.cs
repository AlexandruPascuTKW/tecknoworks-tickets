namespace Accounts.Domain.Models.Organizers;

/// <summary>ACC-F5 — Owner or Manager.</summary>
public class TeamRole : Enumeration
{
    public static readonly TeamRole Owner = new(1, nameof(Owner));
    public static readonly TeamRole Manager = new(2, nameof(Manager));

    private TeamRole(int value, string name) : base(value, name)
    {
    }
}
