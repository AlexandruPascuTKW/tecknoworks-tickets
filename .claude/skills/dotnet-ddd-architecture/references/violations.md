# Violation Catalogue — the review rubric

The checklist for **Review** mode. Work top to bottom: the S1 section is where architecture actually dies, and finding one there usually makes the rest of the review moot until it's fixed.

Each entry gives the **symptom** (what you literally look for), **why it matters** (the property being destroyed — say this in the finding, because a rule without a reason gets argued away), and the **fix**. Grep hints are for a quick first pass, not a substitute for reading the code.

## Contents

- [S1 — the boundary is gone](#s1--the-boundary-is-gone)
- [S2 — a layer or encapsulation guarantee is broken](#s2--a-layer-or-encapsulation-guarantee-is-broken)
- [S3 — convention drift](#s3--convention-drift)
- [Things that look like violations but aren't](#things-that-look-like-violations-but-arent)
- [Fast first pass](#fast-first-pass)

---

## S1 — the boundary is gone

### 1.1 One context references another
**Symptom.** `<ProjectReference Include="..\..\<OtherCtx>\...">` in any of a context's four `.csproj` files. Or a `using`/type from another context's namespace.
**Why it matters.** This is the single failure the whole architecture exists to prevent. Once the reference compiles, nothing stops the next hundred usages, and the context can no longer be lifted into its own service or reasoned about alone.
**Fix.** Delete the reference. Route the need through a domain event (write-side reaction), an HTTP call against the other context's public API with a locally-declared DTO (read-side enrichment), or — if the need is *structural* rather than incidental — a new bounded context that owns the shared concern. See `context-design.md`.
**Grep.** `grep -rn "ProjectReference" --include="*.csproj"` and read every path: a context project's references should only ever point at `Common\*` or its own siblings.

### 1.2 A cross-context type smuggled through `Common`
**Symptom.** A business type in `Common.Domain` or `Common.Application` that only exists so two contexts can share it: `Common.Domain/Models/Order.cs`, `Common.Application/Contracts/IOrderService.cs`, a shared `CustomerDto`.
**Why it matters.** The reference graph still looks clean, so this passes review by inspection while coupling every context to every other one. It is how a modular monolith quietly becomes a distributed monolith.
**Fix.** Move the type into its owning context. If both contexts genuinely need the *information*, each declares its own DTO in `<Ctx>.Application/Contracts/<OtherCtx>/`. The only business-shaped types allowed in `Common` are cross-context event contracts.

### 1.3 A domain object inside a cross-context event
**Symptom.** `Common.Domain/Events/.../OrderPlacedEvent.cs` with a property typed `Order`, `Product`, `Price`, or any other aggregate/value object.
**Why it matters.** The event contract is the seam. Putting a model inside it drags that model into the shared kernel, which means both contexts now compile against it — the coupling you removed at the project level reappears at the type level, and a rename in the producer breaks every subscriber.
**Fix.** Primitives only: `Guid OrderId`, `decimal Amount`, `string Currency`. If the subscriber needs more, either widen the event with more primitives or have the subscriber fetch what it needs over HTTP.

### 1.4 Domain depends on Infrastructure, Application, or Web
**Symptom.** In `<Ctx>.Domain`: a reference to any project other than `Common.Domain`; `using Microsoft.EntityFrameworkCore`; `using MediatR`; `[Key]` / `[Required]` / `[Column]` / `[Table]` attributes on an entity; `IConfiguration`; `HttpClient`; `virtual` properties added for lazy-loading proxies.
**Why it matters.** The domain is the only layer with no substitutes. Once it knows about EF, the model's shape starts answering to the ORM instead of the business, it can't be unit-tested without a provider, and the persistence choice becomes permanent.
**Fix.** Attributes → Fluent API in `<Ctx>.Infrastructure/Configurations/`. Framework needs → an interface in Application implemented in Infrastructure.
**Grep.** `grep -rn "EntityFrameworkCore\|MediatR\|Microsoft.AspNetCore\|\[Key\]\|\[Required\]\|\[Column\]" <Ctx>.Domain/`

### 1.5 Application or Web reaching into Infrastructure
**Symptom.** `DbContext` injected into a MediatR handler; `<Ctx>.Web` referencing `<Ctx>.Infrastructure`; a controller injecting a repository or a concrete HTTP service; `AddScoped<Concrete>` inside a Web project.
**Why it matters.** It inverts the dependency the architecture is named after. Handlers become untestable without a database, and controllers acquire opinions about storage that make the HTTP layer impossible to change independently.
**Fix.** Handlers depend on `I<Aggregate>DomainRepository` / `I<Aggregate>QueryRepository`. Controllers depend on nothing but `Send(...)`. Concrete registration happens in `ProjectStartup`, the only project allowed to see both sides.

### 1.6 One transaction, two contexts
**Symptom.** A handler resolving two `DbContext`s, or a shared `DbContext` with `DbSet`s from two contexts, or `TransactionScope` spanning two contexts' repositories.
**Why it matters.** A transaction is a consistency boundary. Sharing one makes two contexts one context with extra folders, and guarantees they can never be deployed or scaled apart.
**Fix.** One context writes; the other reacts to a domain event and accepts eventual consistency. If the business genuinely cannot tolerate that, the two aggregates belong in one context — change the boundary rather than the transaction.

### 1.7 Two contexts writing the same table
**Symptom.** Two `IEntityTypeConfiguration`s in different contexts mapping the same table name; a migration in one context altering another's table; a foreign key across a context boundary.
**Why it matters.** Shared-table ownership is boundary violation at the data layer, invisible in C#. It is the hardest kind to unwind, because both contexts' migrations now depend on a schema neither owns.
**Fix.** Exactly one context owns each table and its migrations. Other contexts hold a reference by id, or a denormalized copy maintained from events.

---

## S2 — a layer or encapsulation guarantee is broken

### 2.1 Public aggregate constructor / creation bypassing the factory
**Symptom.** `public Product(...)` on an aggregate root; a handler doing `new Order(...)` instead of using `IOrderFactory`.
**Why it matters.** The factory is the anti-corruption layer for creation — it enforces *completeness* (every required part supplied) where the constructor enforces *validity* (each part legal). Bypassing it lets a partially-built aggregate exist, and every downstream invariant is then a maybe.
**Fix.** Constructor `internal`, creation via `I<Aggregate>Factory`. The `internal` non-validating constructor EF needs for materialization is fine and expected.

### 2.2 Anemic aggregate — public setters, logic in the handler
**Symptom.** `public string Name { get; set; }` on an entity; a handler computing totals, checking state transitions, or assigning several properties in sequence; a `<Ctx>Service` in Application holding the rules.
**Why it matters.** This is the most common way a solution keeps a perfect dependency graph and loses the point of having a domain layer. Rules scattered across handlers get re-implemented slightly differently in the next handler, and the model stops being the place you go to learn how the business works.
**Fix.** Private setters; intention-named chainable methods (`UpdateStatus`, `AddOrderItem`) that validate; the handler orchestrates only — load, call, save.

### 2.3 Missing or misplaced invariant validation
**Symptom.** A rule enforced only by a FluentValidation validator; two primitives that always travel together (amount+currency, value+unit) as separate scalar properties; a length limit hardcoded rather than read from `<Aggregate>ModelConstants`.
**Why it matters.** Application validators only see requests. Anything reaching the model another way — an event handler, a seed, a migration, a future second entry point — bypasses them entirely. The domain must hold regardless of how it was called.
**Fix.** `Guard` in the constructor/mutator, or a value object that cannot be constructed invalid. Keep the Application validator too — it produces good 400s — and have both read the same constants.

### 2.4 `IQueryable` or `DbSet` escaping Infrastructure
**Symptom.** A repository interface or method returning `IQueryable<T>`; `All()` made `public`; a handler composing `.Where(...).Include(...)`.
**Why it matters.** It leaks the abstraction: the caller now decides when SQL runs and what the provider must translate, so an ORM change or a lazy-loading surprise becomes an Application-layer problem. Deferred execution also escapes the `DbContext`'s lifetime.
**Fix.** Return materialized entities, DTOs, or `Task<List<T>>`. Keep `All()` / `AllAsNoTracking()` `protected`. Push predicates down with `Specification<T>` when the caller needs to express a filter.

### 2.5 Domain entity used as an API contract
**Symptom.** A controller action returning `ActionResult<Product>`; a command with a property typed as an entity or value object; `IRequest<Order>`.
**Why it matters.** It couples the wire format to the model, so every internal rename is a breaking API change, and it over-posts — any settable property becomes writable by any caller.
**Fix.** `<Aggregate>Response` / `<Aggregate>ListItem` out, `<Aggregate>Command` in, mapped via `IMapFrom<T>` and `ProjectTo`.

### 2.6 Missing `internal` on Infrastructure types
**Symptom.** `public class ProductRepository`, `public class ProductDbContext`, `public class ProductConfiguration`, `public class ProductDbInitializer`.
**Why it matters.** Public Infrastructure types are an invitation. They are also the only way a *future* cross-context reference becomes usable — closing the door now removes the temptation entirely.
**Fix.** `internal`. They are resolved through interfaces and discovered by assembly scan; nothing needs them public.

### 2.7 Wrong failure channel
**Symptom.** `Result.Failure("not found")` where the entity is missing; an exception thrown for a business outcome the caller is expected to handle; a `try/catch` in a controller; a raw `Exception`/`InvalidOperationException` crossing a layer.
**Why it matters.** Two channels with clear meanings make the HTTP surface predictable and let `Common.Web`'s handlers produce consistent ProblemDetails. Mixing them means every caller has to check both.
**Fix.** `NotFoundException` for a missing entity; `Result` / `Result<T>` for expected business failure; `Guard` or a `BaseDomainException` subclass for a violated invariant. Controllers never catch — the registered exception handlers do.

### 2.8 Interfaces with one implementation and no boundary
**Symptom.** `IProductService` / `ICreateProductHandler` / `IOrderManager` in Application wrapping a handler; an interface whose only implementation lives in the same project.
**Why it matters.** Abstraction without a substitution point is pure indirection — more files, more navigation, no seam. MediatR already resolves handlers by request type; it does not need a handler interface.
**Fix.** Delete it. The test for keeping an interface: does it cross a process boundary or need mocking in a test? Repositories, HTTP clients, mail, clock, blob storage — yes. A handler wrapper — no.

### 2.9 Event handler doing cross-context writes directly
**Symptom.** An `IEventHandler<T>` in context B writing through context A's repository, or an event handler that resolves a `DbContext` it doesn't own.
**Why it matters.** It reintroduces the coupling the event was there to break, and turns an eventual-consistency seam back into a hidden distributed transaction.
**Fix.** The handler writes only its own context's aggregates. If it needs data it doesn't have, put the data in the event payload as primitives, or fetch it over HTTP.

### 2.10 Persistence bending the model
**Symptom.** An entity property that exists only for EF (a raw FK alongside a navigation, an `int` where an `Enumeration` belongs, a `virtual` for proxies); a value object flattened into scalars on the entity because owned-type mapping was fiddly; a public parameterless constructor added "for EF".
**Why it matters.** The most insidious form of rot: every project reference is legal, every arch test passes, and the model is slowly becoming a table. Once it's a table, the domain layer is overhead rather than value.
**Fix.** Express the mapping in `Configurations/` — `OwnsOne` for value objects, `HasMany`/`UsingEntity` for joins, backing fields where needed. EF can bind to a private/`internal` constructor; it does not need a public one.

---

## S3 — convention drift

Low individual cost, high cumulative cost: consistent structure is what lets a reviewer spot an S1 by glancing at a file path.

| # | Symptom | Fix |
| --- | --- | --- |
| 3.1 | Handler in its own file instead of nested in its command/query | Nest it. One slice, one file. |
| 3.2 | File in the wrong folder (`Services/` holding a use case, a query under `Commands/`) | Move per `layer-map.md`. |
| 3.3 | Naming off-pattern (`ProductCreateCommand`, `GetProductQuery`, `ProductDto`) | `Create<Aggregate>Command`, `<Aggregate>DetailsQuery`, `<Aggregate>Response`. |
| 3.4 | Validation constants duplicated between validator, entity, and EF config | All three read `<Aggregate>ModelConstants`. |
| 3.5 | Two repository interfaces collapsed into one, or reads going through the domain repository | Split: `I<Aggregate>DomainRepository` (Domain, domain objects) / `I<Aggregate>QueryRepository` (Application, DTOs). |
| 3.6 | Manual DI registration where a scan exists (factories, event handlers, repositories, validators, EF configs, AutoMapper profiles) | Let the scan find it; conform to the marker interface instead. |
| 3.7 | Package version pinned in a `.csproj` | Move to `Directory.Packages.props`. |
| 3.8 | Child entity marked `IAggregateRoot` | Only the root. |
| 3.9 | New context missing from `Program.cs`, the migration script, or the arch tests | Complete the checklist at the end of `layer-map.md`. |
| 3.10 | Cross-context HTTP fan-out in a hot read path with no comment | Note the N+1, or move to event-driven denormalization. |

---

## Things that look like violations but aren't

Flagging these costs you credibility, and credibility is what makes the S1 findings land.

- **The same DTO shape declared in two contexts.** Deliberate. `OrderManagement.Application/Contracts/Products/ProductResponse.cs` duplicating two fields of ProductCatalog's response *is* the anti-corruption layer. Do not suggest extracting it to `Common`.
- **`Infrastructure` referencing `Application`.** Correct in this architecture — Infrastructure implements both the Domain-side and Application-side repository interfaces, so it must see both.
- **A second `internal` constructor on an aggregate that skips validation.** That's EF materialization. Fine as long as it is `internal` and the public path is the factory.
- **A god-looking `Common.Domain` full of base classes.** Plumbing in the shared kernel is the point; only *business* types there are a finding.
- **`Result` implicit conversions.** Intentional ergonomics on the base type, not sloppiness.
- **Global namespace (no `namespace` declaration) in the template's projects.** A convention of this codebase, enabled by the project-level isolation. Match the surrounding files rather than "fixing" it.
- **Aggregate mutators returning `this`.** Intentional chaining, not a fluent-interface smell.
- **Seed data (`<Entity>.Data.cs`) constructing entities directly in Domain.** It lives inside the domain assembly, so it can legitimately use the `internal` constructor.

---

## Fast first pass

Cheap commands that surface most S1/S2 findings before you read any C#. Adjust paths to the solution.

```bash
# Quote the --include globs: unquoted, zsh tries to expand them and the command fails.

# 1.1 — cross-context project references: every path must be Common\* or a sibling
grep -rn "ProjectReference" --include="*.csproj" .

# 1.4 — infrastructure/framework leaking into Domain
grep -rln "EntityFrameworkCore\|MediatR\|Microsoft.AspNetCore\|HttpClient" --include="*.cs" */*.Domain/

# 1.5 — DbContext escaping Infrastructure
grep -rn "DbContext" --include="*.cs" */*.Application/ */*.Web/

# 2.1 / 2.2 — public aggregate constructors and public setters on entities
grep -rn "public .*(\|{ get; set; }" --include="*.cs" */*.Domain/Models/

# 2.4 — IQueryable crossing a boundary
grep -rn "IQueryable" --include="*.cs" */*.Application/ */*.Domain/

# 2.6 — Infrastructure types that should be internal
grep -rn "^public \(sealed \)\?class" --include="*.cs" */*.Infrastructure/

# 1.2 / 1.3 — business types and fat events in the shared kernel
ls -R Common/Common.Domain/Models Common/Common.Domain/Events
```

If a diff touches no `.csproj` and no `Common/` file, the reference graph and shared kernel are intact — go straight to the type-level and layer checks.
