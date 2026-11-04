# Layer Map — what goes where, and what it's called

The authoritative placement and naming reference. Read this in **Plan** mode before naming files, and in **Review** mode to check whether a file is in the right place.

## Contents

- [Solution tree](#solution-tree)
- [Domain layer](#domain-layer)
- [Application layer](#application-layer)
- [Infrastructure layer](#infrastructure-layer)
- [Web layer](#web-layer)
- [The Common shared kernel](#the-common-shared-kernel)
- [ProjectStartup](#projectstartup)
- [Base types you inherit from](#base-types-you-inherit-from)
- [Naming conventions](#naming-conventions)
- [Visibility rules](#visibility-rules)
- [Adding a new bounded context — checklist](#adding-a-new-bounded-context--checklist)

---

## Solution tree

One directory per bounded context at the repository root; one solution folder per context in the `.sln`, so the boundary is visible in the IDE as well as on disk.

```
<Solution>/
  Common/                       # shared kernel — see below, keep it thin
    Common.Domain/
    Common.Application/
    Common.Infrastructure/
    Common.Web/
  <Ctx>/                        # one per bounded context
    <Ctx>.Domain/
    <Ctx>.Application/
    <Ctx>.Infrastructure/
    <Ctx>.Web/
  ProjectStartup/               # the single host — the only multi-context project
  tests/
    <Ctx>.Application.Tests/
    <Ctx>.Domain.Tests/
    ArchitectureTests/          # see arch-tests.md
  Directory.Packages.props      # central package versions — versions live here, not in csproj
  global.json
```

Package references in a `.csproj` carry **no version attribute**; versions are pinned centrally in `Directory.Packages.props`. A version in a `.csproj` is a review finding.

## Domain layer

`<Ctx>.Domain` — the business model. References `Common.Domain` and nothing else. No EF, no MediatR, no ASP.NET, no `IConfiguration`.

```
<Ctx>.Domain/
  Models/
    <Aggregate>s/                          # folder per aggregate, plural
      <Aggregate>.cs                       # : Entity, IAggregateRoot — internal ctor
      <Aggregate>.Data.cs                  # <Aggregate>Data : IInitialData — seed data
      <Child>.cs                           # : Entity — child entity inside the aggregate
      <ValueObject>.cs                     # : ValueObject
      <Enum>.cs                            # : Enumeration
      ModelConstants.cs                    # <Aggregate>ModelConstants — nested class per model
  Factories/
    I<Aggregate>Factory.cs                 # : IFactory<<Aggregate>> — public
    <Aggregate>Factory.cs                  # internal
  Repositories/
    I<Aggregate>DomainRepository.cs        # : IDomainRepository<<Aggregate>>
  Exceptions/
    <Ctx>Exception.cs                      # : BaseDomainException, when guards aren't enough
  DomainConfiguration.cs                   # Add<Ctx>Domain(this IServiceCollection)
```

Notes that matter in review:

- **Aggregate root** is the only type marked `IAggregateRoot`. Child entities inherit `Entity` but not `IAggregateRoot`, so they can never be loaded or saved independently.
- **Mutation is intention-named and chainable.** `UpdateName(string)` returning `this`, not `public string Name { get; set; }`. Every setter is `private`.
- **Value objects are immutable**, validate in their constructor, and implement `GetEqualityComponents()`. Use one whenever two primitives always travel together (amount+currency, value+unit) or a primitive has rules of its own.
- **`Enumeration`** rather than a C# `enum` when the value needs a name, persistence, or lookup by value.
- **`ModelConstants`** is the single source for lengths and ranges, consumed by domain guards, Application validators, and EF configurations alike.

## Application layer

`<Ctx>.Application` — use-case orchestration. References `Common.Application` and `<Ctx>.Domain`. MediatR, FluentValidation, AutoMapper. **No EF, no `DbContext`, no HTTP client implementations, no ASP.NET types.**

```
<Ctx>.Application/
  <Aggregate>s/
    Common/
      <Aggregate>Model.cs                  # shared request/response shape, : IMapFrom<<Aggregate>>
    Commands/
      Common/
        <Aggregate>Command.cs              # : <Aggregate>Model — the shared command body
        <Aggregate>CommandValidator.cs     # : AbstractValidator<<Aggregate>Command>
      Create/
        Create<Aggregate>Command.cs        # : <Aggregate>Command, IRequest<Create<Aggregate>Response>
                                           #   + nested Create<Aggregate>CommandHandler
        Create<Aggregate>CommandValidator.cs   # Include(new <Aggregate>CommandValidator())
        Create<Aggregate>Response.cs       # internal ctor
      Update/
        Update<Aggregate>Command.cs        # : <Aggregate>Command, IRequest<Result>
        Update<Aggregate>CommandValidator.cs
      Delete/
        Delete<Aggregate>Command.cs        # : EntityCommand, IRequest<Result>
    Queries/
      Common/
        <Aggregate>Response.cs             # : <Aggregate>Model — the read shape
      Details/
        <Aggregate>DetailsQuery.cs         # : EntityCommand, IRequest<<Aggregate>Response>
      GetAll/
        GetAll<Aggregate>sQuery.cs
        <Aggregate>ListItem.cs
      I<Aggregate>QueryRepository.cs       # : IQueryRepository<<Aggregate>>
  Contracts/
    <OtherCtx>/
      <Type>Response.cs                    # locally-declared shape of another context's data
  Services/
    I<OtherCtx>HttpService.cs              # interface only; implementation is Infrastructure
  Handlers/
    <Event>EventHandler.cs                 # : IEventHandler<<Event>> — inbound from other contexts
  Settings/
    <Ctx>Settings.cs
  <Ctx>ApplicationConfiguration.cs         # Add<Ctx>Application(services, configuration)
```

Notes that matter in review:

- **One folder per slice; the handler is a nested class inside its command or query.** `CreateProductCommand.cs` contains both `CreateProductCommand` and `CreateProductCommandHandler`. This is deliberate — the request and the only code that handles it stay in one file, and there is nowhere to accumulate a god-service.
- **No `I<Something>Service` in Application unless it crosses a process boundary** and needs mocking (HTTP clients, mail, blob storage, clock). An interface invented so a handler can delegate to a service is a layer that shouldn't exist — the handler *is* the use case.
- **`Contracts/<OtherCtx>/`** holds the consumer's own declaration of another context's payload. Never a reference to the producing context's type.
- **`Handlers/`** is where a context reacts to another context's events. This folder is the legitimate inbound seam.
- **`Result` / `Result<T>`** as the response type when the use case can fail in a way the caller must handle; a plain DTO when it can't.

## Infrastructure layer

`<Ctx>.Infrastructure` — everything that talks to the outside. References `Common.Infrastructure` and `<Ctx>.Application` (so it can implement both repository interfaces). Almost every type here is `internal`.

```
<Ctx>.Infrastructure/
  Persistence/
    <Ctx>DbContext.cs                      # internal : BaseDbContext<<Ctx>DbContext>
    <Ctx>DbInitializer.cs                  # internal : DbInitializer
  Configurations/
    <Aggregate>Configuration.cs            # internal : IEntityTypeConfiguration<<Aggregate>>
  Repositories/
    <Aggregate>Repository.cs               # internal : DataRepository<<Ctx>DbContext, <Aggregate>>,
                                           #   I<Aggregate>DomainRepository, I<Aggregate>QueryRepository
  HttpServices/
    <OtherCtx>HttpService.cs               # : I<OtherCtx>HttpService from Application
  Services/
    <External>Service.cs
  Migrations/                              # EF migrations, per context
  Extensions/
    ConfigurationExtensions.cs
  InfrastructureConfiguration.cs           # Add<Ctx>Infrastructure(services, configuration)
```

Notes that matter in review:

- **One `DbContext` per context**, always. A `DbContext` with two contexts' `DbSet`s is the boundary collapsing, even if every project reference is still legal.
- **`OnModelCreating` calls `ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`** — configurations are discovered, never registered by hand.
- **Repositories return materialized results or DTOs.** `IQueryable<T>` never leaves this project; `All()` / `AllAsNoTracking()` are `protected` on the base for exactly that reason.
- **Read projections use `mapper.ProjectTo<TResponse>(...)`** so the shaping happens in SQL.
- **Migrations belong to the context's own Infrastructure assembly** (`MigrationsAssembly(typeof(TDbContext).Assembly.FullName)`), which is what makes lifting a context out into its own service a mechanical operation.

## Web layer

`<Ctx>.Web` — HTTP surface. References `Common.Web` and `<Ctx>.Application`. **Never `<Ctx>.Infrastructure`.**

```
<Ctx>.Web/
  Features/
    <Aggregate>sController.cs              # : ApiController
  WebConfiguration.cs                      # Add<Ctx>WebComponents(this IServiceCollection)
```

A controller action binds a MediatR request and calls `Send`. That is the whole job:

```csharp
[HttpPost]
public async Task<ActionResult<CreateProductResponse>> Create(CreateProductCommand command)
    => await Send(command);
```

Anything else in a controller — a repository, a `DbContext`, an `if` on business state, mapping, a service call — is a finding. Route templates come from `ApiController`'s `[Route("api/[controller]/[action]")]`; the `Id` constant supplies `{id}`.

## The Common shared kernel

Shared plumbing, and the **only** legal home for a type two contexts both need. Every addition here couples every context, so the bar is high: add to `Common` only when the type is either infrastructure-shaped (a base class, a pipeline behavior, an exception handler) or a deliberate cross-context event contract.

```
Common/
  Common.Domain/
    Models/          Entity · ValueObject · Enumeration · Guard · IEntity · CommonModelConstants
    Events/
      <Aggregate>/   <Event>.cs               # cross-context domain event contracts
    IAggregateRoot · IDomainRepository<T> · IRepository<T> · IFactory<T> · IInitialData
    Specification<T> · BaseDomainException · DomainConfiguration
  Common.Application/
    Behaviours/      RequestValidationBehavior<,>
    Contracts/       ICurrentUser · IQueryRepository<T> · IImageService
    Exceptions/      NotFoundException · ModelValidationException
    Mapping/         IMapFrom<T> · MappingProfile
    Result · EntityCommand · IEventHandler<T> · ApplicationConfiguration
  Common.Infrastructure/
    DBContext/       BaseDbContext<T>          # dispatches domain events after SaveChangesAsync
    Events/          IEventDispatcher · EventDispatcher
    Persistence/     IDbInitializer · DbInitializer
    Repositories/    DataRepository<TDbContext, TEntity>
    InfrastructureConfiguration                # AddDBStorage · AddTokenAuthentication · AddEventSourcing
  Common.Web/
    ApiController · ExceptionHandlers/ · Extensions/ResultExtensions · WebConfiguration
```

**A business type in `Common` is a review finding**, with one exception: a cross-context domain event, which must be a marker or carry primitives only. `Common.Domain/Events/Orders/OrderAddedEvent.cs` is legal. `Common.Domain/Models/Order.cs` is the shared kernel becoming a distributed monolith.

## ProjectStartup

Composition only. Per context, in dependency order:

```csharp
builder.Services
    .Add<Ctx>Domain()
    .Add<Ctx>Application(builder.Configuration)
    .Add<Ctx>Infrastructure(builder.Configuration)
    .Add<Ctx>WebComponents();
```

Then the cross-cutting registrations once (`AddTokenAuthentication`, `AddEventSourcing`, `AddModelBinders`, `AddExceptionHandling`, Swagger, `AddHttpClient`), then `UseWebService(...)` and `Initialize()`. A new context adds exactly four chained lines here and nothing else.

## Base types you inherit from

| Base / interface | Where | Purpose |
| --- | --- | --- |
| `Entity` | `Common.Domain.Models` | `Guid Id`, identity equality, `RaiseEvent` / `Events` / `ClearEvents` |
| `IAggregateRoot` | `Common.Domain` | marks the transactional boundary — roots only |
| `ValueObject` | `Common.Domain.Models` | structural equality via `GetEqualityComponents()`, EF-proxy-safe |
| `Enumeration` | `Common.Domain.Models` | value+name, `FromValue<T>(int)` |
| `Guard` | `Common.Domain.Models` | `AgainstEmptyString` · `ForStringLength` · `AgainstOutOfRange` · `Against` |
| `IFactory<TEntity>` | `Common.Domain` | `TEntity Build()` |
| `IDomainRepository<TEntity>` | `Common.Domain` | `Save(entity, ct)` |
| `IQueryRepository<TEntity>` | `Common.Application` | marker for the read side |
| `IInitialData` | `Common.Domain` | `EntityType` + `GetData()` seed data |
| `Specification<T>` | `Common.Domain` | composable `And`/`Or` predicates, implicitly castable to an expression |
| `EntityCommand` | `Common.Application` | `Guid Id` — base for id-only commands and detail queries |
| `Result` / `Result<TData>` | `Common.Application` | success/failure with errors, implicit conversions |
| `IMapFrom<T>` | `Common.Application.Mapping` | AutoMapper profile discovered by assembly scan |
| `IEventHandler<TEvent>` | `Common.Application` | `Handle(domainEvent)`, registered by Scrutor |
| `BaseDbContext<TContext>` | `Common.Infrastructure` | saves, then dispatches raised domain events |
| `DataRepository<TDbContext, TEntity>` | `Common.Infrastructure` | `Save`, `protected All()` / `AllAsNoTracking()` |
| `DbInitializer` | `Common.Infrastructure` | migrate + seed on startup |
| `ApiController` | `Common.Web` | `[ApiController]`, route template, `Send(...)` overloads |
| `BaseDomainException` | `Common.Domain` | domain errors mapped to ProblemDetails by `DomainExceptionHandler` |

## Naming conventions

| Thing | Pattern | Example |
| --- | --- | --- |
| Context directory & project prefix | PascalCase business name | `ProductCatalog`, `OrderManagement` |
| Aggregate folder | plural | `Models/Products/`, `Products/` |
| Command | `<Verb><Aggregate>Command` | `CreateProductCommand` |
| Command handler | `<Command>Handler`, nested in the command | `CreateProductCommandHandler` |
| Validator | `<Command>Validator` | `UpdateProductCommandValidator` |
| Query | `<Aggregate><Purpose>Query` / `GetAll<Aggregate>sQuery` | `ProductDetailsQuery` |
| Command response | `<Command minus "Command">Response` | `CreateProductResponse` |
| Read response | `<Aggregate>Response`, list rows `<Aggregate>ListItem` | `ProductResponse` |
| Shared model | `<Aggregate>Model` | `ProductModel` |
| Factory | `I<Aggregate>Factory` / `<Aggregate>Factory` | `IProductFactory` |
| Domain repository | `I<Aggregate>DomainRepository` | `IProductDomainRepository` |
| Query repository | `I<Aggregate>QueryRepository` | `IProductQueryRepository` |
| Repository impl | `<Aggregate>Repository` | `ProductRepository` |
| DbContext | `<Ctx>DbContext` | `OrderManagementDbContext` |
| EF configuration | `<Entity>Configuration` | `OrderItemConfiguration` |
| Controller | `<Aggregate>sController` in `Features/` | `ProductsController` |
| Event | `<Aggregate><PastTenseVerb>Event` | `OrderAddedEvent` |
| Event handler | `<Event>Handler` | `OrderAddedEventHandler` |
| DI extension | `Add<Ctx><Layer>` | `AddProductCatalogInfrastructure` |
| Constants | `<Aggregate>ModelConstants` in `ModelConstants.cs`, nested class per model | `ProductModelConstants.Product.MaxNameLength` |
| Seed data | `<Entity>Data` in `<Entity>.Data.cs` | `ProductData` |

## Visibility rules

Encapsulation is the second enforcement mechanism after project references. These are not stylistic.

| Type | Visibility | Why |
| --- | --- | --- |
| Aggregate constructor | `internal` | forces creation through the factory, so a half-valid aggregate cannot exist |
| Factory implementation | `internal class` | consumers depend on `I<Aggregate>Factory` |
| Entity property setters | `private` | state changes go through intention-named methods that validate |
| `DbContext` | `internal` | nothing outside Infrastructure may touch persistence |
| Repository implementation | `internal` | resolved through its interfaces |
| EF `IEntityTypeConfiguration` | `internal` | discovered by assembly scan |
| `DbInitializer` | `internal` | registered as `IDbInitializer` |
| Command response constructor | `internal` | only the handler constructs the response |
| Value object constructor | `public` or `internal` | either is fine — it validates itself and is immutable |
| Factory interface, repository interfaces, commands, queries, responses | `public` | they are the cross-layer contract |

## Adding a new bounded context — checklist

1. Create `<Ctx>/` with the four projects and the reference edges from the dependency table.
2. Add a solution folder named `<Ctx>` in the `.sln` and nest the four projects under it.
3. `DomainConfiguration.Add<Ctx>Domain()` → `AddCommonDomain(Assembly.GetExecutingAssembly())`.
4. `<Ctx>ApplicationConfiguration.Add<Ctx>Application(services, configuration)` → `AddCommonApplication(configuration, Assembly)`.
5. `InfrastructureConfiguration.Add<Ctx>Infrastructure(...)` → `AddDBStorage<<Ctx>DbContext>(configuration, Assembly.GetExecutingAssembly())` + `AddTransient<IDbInitializer, <Ctx>DbInitializer>()`.
6. `WebConfiguration.Add<Ctx>WebComponents()` → `AddWebComponents(typeof(<Ctx>ApplicationConfiguration))`.
7. Add `<Ctx>.Infrastructure` and `<Ctx>.Web` to `ProjectStartup.csproj`, and the four chained calls to `Program.cs`.
8. Add the context to the migration script and to the arch-test module list (`arch-tests.md`) — a context with no arch test is unprotected from day one.
