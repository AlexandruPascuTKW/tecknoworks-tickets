---
name: dotnet-ddd-architecture
description: >-
  Plan, place, and police code structure in a .NET DDD solution built as a modular monolith of
  bounded contexts, each with its own Clean Architecture layers (Domain / Application /
  Infrastructure / Web) and a single ProjectStartup host. Two jobs: PLAN — decide which context
  owns a capability, whether it needs a new one, and lay out the exact projects, folders, files and
  reference edges before anyone writes code; REVIEW — audit a diff, PR, file, or whole solution for
  boundary and layering violations (a context referencing another context, EF or ASP.NET leaking
  into Domain, a public aggregate constructor bypassing its factory, a domain object smuggled inside
  a cross-context event, a handler reaching into another context's repository), reported with
  severity and the exact fix. Use this skill whenever someone asks where a new feature or file
  should live in a .NET/C# solution, whether something belongs in Domain or Application, how to add
  a bounded context or module, how two contexts should talk (domain event vs HTTP vs shared kernel),
  whether a PR violates the architecture or bounded contexts, why their modules are coupled, how to
  split an overgrown context, how to write NetArchTest architecture/fitness tests, or hands over C#
  code and asks for an architecture review. Also use it proactively before implementing any
  non-trivial feature in such a solution, and when reviewing .NET code that touches entities,
  aggregates, factories, repositories, DbContexts, MediatR handlers, validators, or controllers.
  This is the STRUCTURE-AND-ENFORCEMENT lens on an already-decided set of boundaries — for cutting
  boundaries out of a requirements document in the first place use `requirements-to-domains`.
---

# .NET DDD Architecture — plan it, place it, police it

The architecture this skill serves is a **modular monolith of bounded contexts**. Each context is a vertical slice of the business that carries its own full Clean Architecture stack; all of them compile into one deployable binary through a single host project.

The reason it is built this way is worth holding in mind, because it explains every rule below: **coupling is prevented by structure, not by discipline.** Guidelines telling developers "don't call into the other domain" lose to deadlines. A missing project reference does not. So each context is a set of separate .NET projects, and the friction of adding a forbidden `<ProjectReference>` is the enforcement mechanism. Meanwhile everything runs in one process, so the team pays none of the microservice tax — no service discovery, no orchestration, no internal NuGet feeds — and can still lift a context out into its own repository later by moving a folder and writing one migration script.

Your job is to protect that property. Almost every violation you will find or prevent is someone trading a structural boundary for five minutes of convenience.

**Reference implementation:** `~/dev/ddd/ASP.NET-Domain-Driven-Design-Template`. When a question is about a concrete convention — naming, base types, folder depth — read the corresponding file there rather than guessing. Philosophy: <https://evgenirusev.com/posts/dotnet-ddd-vertical-slice/>.

## Pick your mode

| The ask | Mode | Read |
| --- | --- | --- |
| "Where does this feature go?" / "Plan the architecture for X" / "Add a module for Y" | **Plan** | this file + `references/layer-map.md` |
| "Review this PR/diff/file/solution" / "Are we violating bounded contexts?" | **Review** | this file + `references/violations.md` |
| "Write the code" / "Scaffold the slice" | **Scaffold** | `references/slice-templates.md` |
| "How should these two contexts talk?" / "Is this one context or two?" / "Split this context" | **Context design** | `references/context-design.md` |
| "Make the rules automatic" / "Architecture tests" / "Fitness functions" | **Enforce** | `references/arch-tests.md` |

Modes compose. A planning answer that names a cross-context call should also settle the seam (context design). A review that finds the same violation three times should offer the arch test that would have caught it.

## The dependency graph — the one thing to get right

Everything else in this skill is downstream of this table. `<Ctx>` is a bounded context, e.g. `ProductCatalog`.

| Project | May reference | May **never** reference | Package budget |
| --- | --- | --- | --- |
| `<Ctx>.Domain` | `Common.Domain` | anything else at all | none beyond DI + Scrutor |
| `<Ctx>.Application` | `Common.Application`, `<Ctx>.Domain` | Infrastructure, Web, any other context | MediatR, FluentValidation, AutoMapper |
| `<Ctx>.Infrastructure` | `Common.Infrastructure`, `<Ctx>.Application` | Web, any other context | EF Core, HTTP, storage SDKs |
| `<Ctx>.Web` | `Common.Web`, `<Ctx>.Application` | **`<Ctx>.Infrastructure`**, any other context | ASP.NET, Swashbuckle |
| `ProjectStartup` | every `<Ctx>.Infrastructure` + `<Ctx>.Web` | — | EF Design/Tools |

The `Common.*` projects chain the same way: `Common.Domain ← Common.Application ← Common.Infrastructure ← Common.Web`.

Three consequences people get wrong:

- **Web does not reference Infrastructure.** Controllers know MediatR requests and nothing about persistence. DI wiring lives in `ProjectStartup`, which is the only project allowed to see both. If you find yourself wanting that reference, you are registering a service in the wrong place.
- **`ProjectStartup` is the only project that sees more than one context.** It is a wiring file, not a place for logic. Anything with an `if` in it does not belong there.
- **`<CtxA>.* → <CtxB>.*` is never legal, at any layer, for any reason** — including "I just need one service" and "it's only a DTO". That reference is the architecture failing. Route it through an event or an HTTP call (`references/context-design.md`).

## Plan mode

Answer in this order. Stop and say so if an earlier answer makes a later one moot.

**1. Which context owns this?** Ownership follows the data and the invariant, not the screen. The context that owns the write owns the capability. If a capability wants to write two contexts' tables in one transaction, the boundary is wrong or the capability is two capabilities.

**2. Does it need a new context?** A use case that *consistently* spans two contexts is not a coupling problem to solve — it is a third context asking to exist. `Statistics` in the template is exactly that: rather than OrderManagement and ProductCatalog both maintaining counters, a context subscribes to both their events and owns the counters itself. Apply the merge/split/language/autonomy tests in `references/context-design.md`; for cutting boundaries from scratch out of a requirements document, that is `requirements-to-domains`' job, not this skill's.

**3. Which layer holds each piece?** The test is *what would break this code*:
- a change in **business rules** → `Domain`
- a change in **use-case orchestration** → `Application`
- a change in **a vendor, driver, or wire format** → `Infrastructure`
- a change in **the HTTP contract** → `Web`

**4. Name every file.** Produce the actual tree with actual filenames, following `references/layer-map.md`. A plan that stops at "add a service in Application" has not planned anything — the value is in committing to `Products/Commands/Create/CreateProductCommand.cs` so the next reviewer can tell at a glance whether the convention held.

**5. Settle the seams.** For each cross-context need: event, HTTP, or new context — and what the contract type is and where it lives.

**6. Name the invariants and where each is enforced.** Every rule lands in the Domain (guard or value object). Say which ones *also* need an Application validator (shape/input) and an EF Fluent API constraint (storage), all three reading the same `<Aggregate>ModelConstants`.

### Plan output shape

```markdown
## Ownership
<context> owns this because <data/invariant it owns>. [Or: this warrants a new context <Name> because …]

## Files
<the tree, per project, with real filenames — new files marked NEW, touched files marked MOD>

## Seams
<cross-context need> → <event | HTTP | none> · contract: <type> in <location>

## Invariants
| Rule | Domain (where) | App validator? | EF constraint? |

## Risks
<anything that will tempt a future developer into a boundary violation, and the arch test that would catch it>
```

## Review mode

Read `references/violations.md` for the full catalogue. The procedure:

1. **Start with the `.csproj` files.** `<ProjectReference>` changes are where architecture actually dies; a cross-context reference or an Infrastructure package in a Domain project outranks anything you would find by reading C#. If a diff touches no `.csproj`, the reference graph is intact and you can move on to type-level checks quickly.
2. **Then check the type-level boundary**, which the reference graph cannot catch: cross-context types reached through `Common.*`, domain objects embedded in shared-kernel events, `IQueryable` escaping a repository, `DbContext` reaching an Application handler.
3. **Then layer purity**, file by file, against the placement rules.
4. **Then the encapsulation rules** the model depends on: aggregate constructors `internal`, creation via factory, DbContext / repository / EF configuration / initializer `internal`, no public setters on entities.
5. **Then convention conformance** — naming, folder depth, nested handler, repository split. Lowest severity, but drift here is what makes the higher-severity violations invisible later.

Verify before you report. A suspected violation you can't tie to a specific line and a specific rule is noise, and noise trains people to skip architecture review. If the code is unusual but defensible — a deliberate `Result`-returning path where the template throws, say — note it as a deviation with the reasoning, not as a defect.

### Review output shape

Order by severity, worst first. One row per distinct violation, not one per occurrence — if a rule is broken in nine files, that is one finding with nine sites.

```markdown
### [S1] <rule broken> — <file:line>
**What:** <the specific code>
**Why it matters:** <the property of the architecture this destroys>
**Fix:** <the concrete change, naming the file it moves to / the seam it routes through>
**Also at:** <other sites>
```

Severity:
- **S1** — the boundary is gone. Cross-context reference, cross-context type leak, Domain depending on Infrastructure. These make the modular monolith a monolith; nothing else on the list matters as much.
- **S2** — a layer or encapsulation guarantee is broken. `DbContext` in Application, public aggregate constructor, `IQueryable` returned from a repository, entity exposed as an API response.
- **S3** — convention drift. Misplaced or misnamed file, handler not nested, validation constant duplicated, missing `internal`.

Close with a one-line verdict on whether the change is safe to merge as-is, and — when the same S1/S2 appeared more than once — the arch test that would make it impossible next time.

## Rules that carry the most weight

Compressed here because they come up in nearly every plan and review. Detail and code in the references.

**Creation goes through factories.** Aggregate constructors are `internal`; `I<Aggregate>Factory` (interface in Domain, `internal` implementation in Domain) is the only way in. The factory validates completeness, the constructor validates invariants. This is the anti-corruption layer that keeps a half-built aggregate from ever existing.

**Two repositories, one implementation.** `I<Aggregate>DomainRepository` lives in `Domain` and returns domain objects; `I<Aggregate>QueryRepository` lives in `Application` and returns response DTOs. One `internal` Infrastructure class implements both. The split is what lets reads bypass the domain model without giving writes a way to bypass it.

**Cross-context events are dumb and shared.** A domain event that crosses a boundary lives in `Common.Domain/Events/<Aggregate>/`, because publisher and subscriber may not reference each other. It carries primitives only — putting a domain object inside it re-couples the contexts through the shared kernel and defeats the whole arrangement. Entities raise events via `RaiseEvent`; `BaseDbContext.SaveChangesAsync` dispatches them after the write; handlers implement `IEventHandler<T>` and are registered by assembly scan.

**Cross-context reads use a local DTO.** The consumer declares its own shape in `<Ctx>.Application/Contracts/<OtherCtx>/`. Yes, that duplicates a couple of fields. The duplication *is* the anti-corruption layer — it is what stops a rename in one context from breaking another. Prefer event-driven denormalization over synchronous HTTP on hot paths; a per-row HTTP enrichment loop is acceptable for admin-style reads and should be commented as such.

**Validation is layered on purpose, from one source of constants.** Domain guards and value objects hold the invariants and run always. Application `AbstractValidator`s check request shape through the MediatR pipeline behavior. EF Fluent API expresses storage constraints. All three read `<Aggregate>ModelConstants`, so a length limit changes in one place.

**Failure has two channels.** `Result` / `Result<T>` for expected business failure the caller should handle; exceptions for broken expectations — `NotFoundException` for a missing entity, a `BaseDomainException` subclass or `Guard` throw for a violated invariant. `Common.Web`'s exception handlers map them to ProblemDetails. Mixing the channels — returning `Result.Failure` for a missing row, throwing for a business rule the caller is expected to handle — makes the API contract unpredictable.

**Nothing in the model knows about EF.** No attributes on entities, no navigation shaped around a join table, no `virtual` for proxies. Mapping is Fluent API in `<Ctx>.Infrastructure/Configurations/`, discovered by `ApplyConfigurationsFromAssembly`. If a persistence concern is visibly bending the model, say so — that is the most common way a Clean Architecture solution rots from the inside while every project reference still looks correct.
