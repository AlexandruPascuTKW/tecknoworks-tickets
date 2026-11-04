# Context Design — boundaries, seams, data, and the exit

Read this when the question is about *contexts* rather than files: is this one context or two, how should these two talk, where does the data live, how do we split something that grew too big.

**Scope note.** This file covers the decisions you make *inside an established solution* — validating a boundary, choosing a seam, splitting an overgrown context. Cutting a fresh set of bounded contexts out of a requirements document is a bigger job with its own method: use the `requirements-to-domains` skill, and the vault's `wiki/engineering/architecture/domain-decomposition.md` for the theory.

## Contents

- [Is this one context or two?](#is-this-one-context-or-two)
- [When a use case spans two contexts](#when-a-use-case-spans-two-contexts)
- [Choosing the seam](#choosing-the-seam)
- [Seam 1 — domain events](#seam-1--domain-events)
- [Seam 2 — HTTP calls](#seam-2--http-calls)
- [Seam 3 — a new context](#seam-3--a-new-context)
- [Data ownership](#data-ownership)
- [Splitting an overgrown context](#splitting-an-overgrown-context)
- [The exit to microservices](#the-exit-to-microservices)

---

## Is this one context or two?

Five tests. They can disagree; when they do, autonomy and language win, because those are the two that predict future pain.

**Autonomy.** Can it serve its own use cases with only its own data plus primitive identifiers from elsewhere? A context that cannot answer its own questions without calling a neighbour on every request is not a context — it is half of one.

**Merge.** Are the two always used together, always deployed together, always changed in the same commit? Then they are one context wearing two folders, and the boundary between them is pure overhead. Merge.

**Split.** Is a *subset* of a context's use cases regularly used entirely on its own — different consumers, different change rhythm, different data? That subset wants to be its own context.

**Language.** Does one word mean two different things in the two places? "Order" in fulfilment (a shipment to pack) is not "Order" in billing (a revenue event). Two meanings is the clearest possible signal of two contexts, each with its own model. Conversely, if one word means exactly the same thing in both, ask why they are separate.

**Data.** Does it own its own tables and their migrations? An entity that two contexts both write is a boundary error regardless of how the code is organised.

## When a use case spans two contexts

This is the decision people get wrong most often, so take it slowly. The question is not "how do I get at that data" — it is **how often** this crossing happens.

- **Occasionally, for reading** → an HTTP call with a locally-declared DTO. Cheap, honest, no new structure.
- **Reactively, on a write** → a domain event. The producer doesn't know the consumer exists.
- **Consistently, as the point of the use case** → **the use case is a third context asking to exist.** This is the insight that keeps this architecture from degrading. If two contexts keep needing to cooperate to answer one question, that question belongs to something that owns it.

`Statistics` in the reference template is the canonical worked example. "How many orders and products were created?" touches both OrderManagement and ProductCatalog. The wrong answers are a cross-reference, or a counter maintained in both places, or a query joining both schemas. The right answer is a third context that subscribes to `OrderAddedEvent` and `ProductAddedEvent` and owns the counters itself — at which point neither producer knows it exists, and all three can be developed, deployed, and reasoned about independently.

## Choosing the seam

| Need | Seam | Contract lives in | Consistency |
| --- | --- | --- | --- |
| React to something that happened | Domain event | `Common.Domain/Events/<Aggregate>/` | eventual |
| Read another context's data, cold path | HTTP + local DTO | `<Ctx>.Application/Contracts/<OtherCtx>/` | read-time, coupled to uptime |
| Read another context's data, hot path | Event → local denormalized copy | event contract + own table | eventual, read is local |
| A capability that is *about* both | New context | its own projects | eventual |
| Genuinely one transaction, non-negotiable | **Same context** | — | immediate |

A useful tiebreaker between event and HTTP: **who has to know about whom?** With an event, the producer knows nothing about the consumer — add a fourth subscriber and the producer never changes. With HTTP, the consumer takes on the producer's uptime, latency, and version. Prefer events for write-side reactions; accept HTTP only where you need data at read time and staleness is unacceptable.

## Seam 1 — domain events

**Where the contract lives.** `Common.Domain/Events/<Aggregate>/<Event>.cs`. It must be in the shared kernel, because neither the publisher nor the subscriber may reference the other. This is the one sanctioned business-shaped type in `Common`, and it carries an obligation: **primitives only.** A domain object in an event payload re-couples both contexts through the shared kernel and undoes the whole arrangement.

**Naming.** `<Aggregate><PastTenseVerb>Event` — `OrderAddedEvent`, `ProductRenamedEvent`. Past tense, because an event is a fact, not a request.

**Publish** — from inside the aggregate, at the moment the fact becomes true:

```csharp
public class Order : Entity, IAggregateRoot
{
    internal Order(Guid customerId, DateTime orderDate)
    {
        // … validate, assign …
        RaiseEvent(new OrderAddedEvent());
    }
}
```

**Dispatch** is automatic: `BaseDbContext.SaveChangesAsync` collects entities with pending events, saves, then dispatches and clears. Ordering matters — the write commits before subscribers run, so a handler never reacts to a fact that didn't persist. Reentrant saves from inside a handler are guarded against re-dispatch, so a handler may safely save its own aggregate.

**Subscribe** — in the consuming context's Application layer, `Handlers/<Event>Handler.cs`:

```csharp
public class OrderAddedEventHandler : IEventHandler<OrderAddedEvent>
{
    private readonly IStatisticsDomainRepository statistics;

    public OrderAddedEventHandler(IStatisticsDomainRepository statistics)
        => this.statistics = statistics;

    public Task Handle(OrderAddedEvent domainEvent)
        => statistics.IncrementOrders();
}
```

Registration is by assembly scan (`AddEventHandlers` in `AddCommonApplication`) — never manual.

**The limits, and say them out loud when you recommend this seam.** In-process dispatch after commit is not transactional with the write and not durable: if the process dies between commit and dispatch, the event is lost, and a throwing handler surfaces as a failure of the original request. That is an acceptable trade for the template's scale. When a subscriber's work must not be lost — money, compliance, anything a customer would notice — say so and move that seam to an outbox: persist the event in the same transaction as the aggregate, dispatch from a background reader. Note it as a known limitation rather than pretending the in-process dispatcher is a message bus.

**A handler writes only its own context's aggregates.** A handler reaching into the publisher's repository is violation 2.9 — the coupling is back, just harder to see.

## Seam 2 — HTTP calls

Three pieces, and the shape matters as much as the fact that it's HTTP:

1. **Interface in `<Ctx>.Application/Services/I<OtherCtx>HttpService.cs`** — so the handler depends on an abstraction it owns.
2. **Local DTO in `<Ctx>.Application/Contracts/<OtherCtx>/<Type>Response.cs`** — the consumer's own declaration of the payload. This duplicates a couple of fields from the producer's response *on purpose*: the duplication is the anti-corruption layer, and it is what stops a rename in the producer from breaking the consumer at compile time. Do not extract it to `Common`.
3. **Implementation in `<Ctx>.Infrastructure/HttpServices/`**, registered as a typed client with a base URL from the context's own settings.

```csharp
services.AddHttpClient<ProductCatalogHttpService>(client =>
        client.BaseAddress = new Uri(settings.ProductCatalogAPIClientSettings.BaseUrl))
    .ConfigureDefaultHttpClientHandler()
    .AddTypedClient<IProductCatalogHttpService, ProductCatalogHttpService>();
```

**Costs to name whenever you propose this seam.** The consumer now owns the producer's uptime and latency, and a per-row enrichment loop is an N+1 in HTTP calls. That is fine for an admin-style read over a bounded result set — comment it as such. For anything hot or unbounded, use the event-driven alternative: subscribe to the producer's events and project the field you need onto your own side at write time, so the read becomes a pure local query. Deduplicate ids before fetching either way.

## Seam 3 — a new context

Warranted when the capability is *about* the relationship between two contexts rather than belonging to either — reporting, statistics, reconciliation, search, notification. Signals:

- both existing contexts would need the same new field or counter
- the capability's questions cross both schemas
- neither context's team would naturally own the code
- you keep being tempted to add a cross-reference "just for this"

The new context subscribes to events from both, owns its own tables, and neither producer learns of its existence. Follow the checklist at the end of `layer-map.md`.

## Data ownership

**One `DbContext` per context, always.** Even when they share a physical database.

Two viable storage strategies:

| | Single database, one `DbContext` per context | Database per context |
| --- | --- | --- |
| Development speed | faster — one connection string, one server | slower — more infrastructure to run locally |
| Boundary safety | relies on discipline (nothing stops a hand-written cross-schema join) | enforced by the database |
| Path to microservices | needs a data migration script per context | move the folder; the data is already apart |
| Recommended for | most teams, most of the time | when a split is already on the roadmap |

Non-negotiables under either strategy:

- **Exactly one context owns each table** and its migrations, which live in that context's Infrastructure assembly.
- **No foreign keys across a context boundary.** Hold the other side's identifier as a plain `Guid`. You lose referential integrity at the database level; you gain the ability to move the context. That is the trade this architecture makes deliberately.
- **Cross-context reads** are an HTTP call or a locally-maintained denormalized copy — never a join.
- **Seed data** per context via `IInitialData` + a `DbInitializer` subclass, run from `ProjectStartup.Initialize()`.

## Splitting an overgrown context

Signals it has outgrown its boundary — two or three together are enough to act:

- unrelated aggregates in one `DbContext` whose `DbSet`s never appear in the same query
- feature folders that never reference each other and never change together
- one word in the context meaning two things depending on the folder
- two teams both touching it, on different rhythms, with migration collisions
- a review that can't answer "does this belong here?" without asking a person

The refactor, in an order that stays green at every step:

1. **Find the seam empirically.** Which aggregates are loaded together in one transaction? Those must stay together — the transactional boundary is the hard constraint. Everything else is negotiable.
2. **Name the two models in the two vocabularies.** If you can't name them differently, you haven't found a split; you've found a folder rearrangement.
3. **Split the folders first, inside the existing projects.** No new projects yet. This is reversible and surfaces every accidental dependency as a `using` you now notice.
4. **Break the remaining code dependencies** across the intended line — each becomes an event, an HTTP call, or a duplicated primitive. This is the real work, and its size tells you whether the split is worth it.
5. **Split the `DbContext`,** then the tables and migrations. One owner each.
6. **Extract the four projects** and wire the new context in `ProjectStartup`.
7. **Add the arch test** asserting the two new contexts don't reference each other — before anyone can accidentally re-couple them.

If step 4 is enormous, stop: the boundary you chose is wrong, or the context genuinely is one thing. A split that requires thirty new events is a merge argument in disguise.

## The exit to microservices

The reason to accept the project-count overhead is that extraction stays mechanical:

1. Move `<Ctx>/` to its own repository, along with the `Common/` projects it needs (as source, or published as an internal NuGet package).
2. Give it its own `ProjectStartup` with only its four registration lines.
3. Migrate its tables to their own database — a script, because it already owned them exclusively.
4. Replace in-process event dispatch across the new process boundary with a real broker; the `IEventHandler<T>` implementations don't change, only what calls them.
5. Point the remaining contexts' typed HTTP clients at the new base URL — which is a configuration change, because they were already going through HTTP.

Nothing in that list requires touching the extracted context's Domain or Application code. That is the property every rule in this skill is protecting; when you review a change, you are asking whether it is still true afterwards.
