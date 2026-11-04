namespace Events.Domain.Models.Teams;

/// <summary>Events' own copy of Accounts' Team roles (ADR-001): Owner or Manager.</summary>
public class TeamRole : Enumeration
{
    public static readonly TeamRole Owner = new(1, nameof(Owner));
    public static readonly TeamRole Manager = new(2, nameof(Manager));

    private TeamRole(int value, string name) : base(value, name)
    {
    }

    /// <summary>EVT-A10 Who can: only an Owner publishes.</summary>
    public bool CanPublish => this == Owner;
}
