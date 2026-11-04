/// <summary>
/// An event whose payload is built when the aggregate is saved, once EF has given every new entity its
/// Id. Lets an aggregate raise an event that mentions the Ids of children it has only just added.
/// </summary>
public sealed class DeferredDomainEvent : IDomainEvent
{
    private readonly Func<IDomainEvent> build;

    public DeferredDomainEvent(Func<IDomainEvent> build) => this.build = build;

    public IDomainEvent Resolve() => build();
}
