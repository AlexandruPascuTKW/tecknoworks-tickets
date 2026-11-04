// Cross-context contracts — primitives only (ADR-005, architecture.md "Seams").

/// <summary>Raised by Accounts when someone joins an Organizer's Team (ACC-A3, ACC-A4).</summary>
public sealed record TeamMemberAddedEvent(Guid OrganizerId, Guid AccountId, string TeamRole) : IDomainEvent;

/// <summary>Raised by Accounts when someone is taken off a Team (ACC-A5).</summary>
public sealed record TeamMemberRemovedEvent(Guid OrganizerId, Guid AccountId) : IDomainEvent;

/// <summary>Raised by Accounts when a Team member's Team role changes (ACC-A6).</summary>
public sealed record TeamRoleChangedEvent(Guid OrganizerId, Guid AccountId, string TeamRole) : IDomainEvent;
