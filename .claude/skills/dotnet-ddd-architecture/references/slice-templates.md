# Slice Templates — a full vertical slice, layer by layer

Working skeletons to adapt in **Scaffold** mode. Substitute `<Ctx>` (bounded context), `<Aggregate>` (aggregate root), `<Vo>` (value object). The template's projects use the global namespace with `ImplicitUsings` — match the surrounding files rather than adding namespace declarations if the target solution does the same.

Order matters: Domain first, then Application, Infrastructure, Web, then registration. Writing a handler before the aggregate exists is how anemic models happen — you end up shaping the model around what the handler already does.

## Contents

- [1. Domain](#1-domain)
- [2. Application](#2-application)
- [3. Infrastructure](#3-infrastructure)
- [4. Web](#4-web)
- [5. Registration](#5-registration)
- [6. Tests](#6-tests)
- [Minimum viable slice](#minimum-viable-slice)

---

## 1. Domain

### `Models/<Aggregate>s/ModelConstants.cs`

Write this first. Everything else reads from it — the entity's guards, the Application validator, the EF configuration.

```csharp
// Models/<Aggregate>s/ModelConstants.cs — one constants class per aggregate folder
public class <Aggregate>ModelConstants
{
    public class <Aggregate>
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 50;
        public const int MinDescriptionLength = 2;
        public const int MaxDescriptionLength = 500;
    }

    public class <Vo>
    {
        public const int MaxValueDigits = 10;
        public const int MaxUnitLength = 10;
    }
}
```

### `Models/<Aggregate>s/<Vo>.cs`

Reach for a value object whenever two primitives always travel together, or a primitive has rules of its own. Immutable, self-validating, equal by value.

```csharp
public class <Vo> : ValueObject
{
    public decimal Value { get; }
    public string Unit { get; }

    internal <Vo>(decimal value, string unit)
    {
        Validate(value, unit);

        Value = value;
        Unit = unit;
    }

    private void Validate(decimal value, string unit)
    {
        Guard.AgainstOutOfRange(value, 0, decimal.MaxValue, nameof(value));
        Guard.AgainstEmptyString(unit, nameof(unit));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }
}
```

### `Models/<Aggregate>s/<Aggregate>.cs`

The `internal` constructor is what forces creation through the factory. Setters are `private`; state changes go through intention-named methods that validate and return `this`.

```csharp
public class <Aggregate> : Entity, IAggregateRoot
{
    internal <Aggregate>(string name, <Vo> measure)
    {
        Validate(name);

        Name = name;
        Measure = measure;

        RaiseEvent(new <Aggregate>AddedEvent());   // only if another context must react
    }

    // EF materialization. internal, no validation — see violations.md "looks like a violation but isn't".
    private <Aggregate>(string name)
    {
        Name = name;
        Measure = default!;
    }

    public string Name { get; private set; }
    public <Vo> Measure { get; private set; }

    public <Aggregate> UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
        return this;
    }

    public <Aggregate> UpdateMeasure(decimal value, string unit)
    {
        Measure = new <Vo>(value, unit);
        return this;
    }

    private void Validate(string name) => ValidateName(name);

    private void ValidateName(string name)
        => Guard.ForStringLength(
            name,
            <Aggregate>ModelConstants.<Aggregate>.MinNameLength,
            <Aggregate>ModelConstants.<Aggregate>.MaxNameLength,
            nameof(Name));
}
```

### `Factories/I<Aggregate>Factory.cs` and `<Aggregate>Factory.cs`

The constructor enforces *validity* of each part; the factory enforces *completeness* of the whole. That division is why both exist.

```csharp
public interface I<Aggregate>Factory : IFactory<<Aggregate>>
{
    I<Aggregate>Factory WithName(string name);
    I<Aggregate>Factory WithMeasure(decimal value, string unit);
}
```

```csharp
internal class <Aggregate>Factory : I<Aggregate>Factory
{
    private string name = default!;
    private <Vo> measure = default!;

    private bool isNameSet;
    private bool isMeasureSet;

    public I<Aggregate>Factory WithName(string name)
    {
        this.name = name;
        isNameSet = true;
        return this;
    }

    public I<Aggregate>Factory WithMeasure(decimal value, string unit)
    {
        measure = new <Vo>(value, unit);
        isMeasureSet = true;
        return this;
    }

    public <Aggregate> Build()
    {
        if (!isNameSet || !isMeasureSet)
            throw new InvalidOperationException("Name and measure must have a value.");

        return new <Aggregate>(name, measure);
    }
}
```

### `Repositories/I<Aggregate>DomainRepository.cs`

The write side. Returns domain objects; lives in Domain.

```csharp
public interface I<Aggregate>DomainRepository : IDomainRepository<<Aggregate>>
{
    Task<<Aggregate>?> Find(Guid id, CancellationToken cancellationToken = default);
    Task Delete(Guid id, CancellationToken cancellationToken = default);
}
```

### `Models/<Aggregate>s/<Aggregate>.Data.cs` *(optional seed)*

```csharp
public class <Aggregate>Data : IInitialData
{
    public Type EntityType => typeof(<Aggregate>);

    public IEnumerable<object> GetData()
        => new List<<Aggregate>>
        {
            new(name: "Sample", measure: new <Vo>(1m, "kg")),
        };
}
```

### `DomainConfiguration.cs`

```csharp
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public static class DomainConfiguration
{
    public static IServiceCollection Add<Ctx>Domain(this IServiceCollection services)
        => services.AddCommonDomain(Assembly.GetExecutingAssembly());
}
```

Factories and `IInitialData` implementations are found by the scan inside `AddCommonDomain`. Do not register them by hand.

---

## 2. Application

### `<Aggregate>s/Common/<Aggregate>Model.cs`

The shared shape both the command and the response inherit, so the write and read contracts can't silently diverge.

```csharp
using AutoMapper;

public record MeasureRequest(decimal Value, string Unit);

public class <Aggregate>Model : IMapFrom<<Aggregate>>
{
    public string Name { get; set; } = default!;
    public MeasureRequest Measure { get; set; } = default!;

    public virtual void Mapping(Profile mapper)
        => mapper.CreateMap<<Aggregate>, <Aggregate>Model>()
            .ForMember(m => m.Measure,
                opt => opt.MapFrom(src => new MeasureRequest(src.Measure.Value, src.Measure.Unit)));
}
```

### `<Aggregate>s/Commands/Common/<Aggregate>Command.cs` + validator

Shared rules live once, on the base command, and each concrete validator `Include`s them. All limits come from `<Aggregate>ModelConstants` — the same constants the entity guards against.

```csharp
public class <Aggregate>Command : <Aggregate>Model
{
}
```

```csharp
using FluentValidation;

public class <Aggregate>CommandValidator : AbstractValidator<<Aggregate>Command>
{
    public <Aggregate>CommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(<Aggregate>ModelConstants.<Aggregate>.MinNameLength,
                    <Aggregate>ModelConstants.<Aggregate>.MaxNameLength);

        RuleFor(c => c.Measure.Value)
            .GreaterThan(CommonModelConstants.Common.Zero)
            .PrecisionScale(<Aggregate>ModelConstants.<Vo>.MaxValueDigits, 2, ignoreTrailingZeros: false);

        RuleFor(c => c.Measure.Unit)
            .NotEmpty()
            .MaximumLength(<Aggregate>ModelConstants.<Vo>.MaxUnitLength);
    }
}
```

### `<Aggregate>s/Commands/Create/Create<Aggregate>Command.cs`

The handler is a nested class. The handler orchestrates and nothing more: build via factory, save, return. Any `if` about business state here belongs on the aggregate.

```csharp
using MediatR;

public class Create<Aggregate>Command : <Aggregate>Command, IRequest<Create<Aggregate>Response>
{
    public class Create<Aggregate>CommandHandler
        : IRequestHandler<Create<Aggregate>Command, Create<Aggregate>Response>
    {
        private readonly I<Aggregate>DomainRepository repository;
        private readonly I<Aggregate>Factory factory;

        public Create<Aggregate>CommandHandler(
            I<Aggregate>DomainRepository repository,
            I<Aggregate>Factory factory)
        {
            this.repository = repository;
            this.factory = factory;
        }

        public async Task<Create<Aggregate>Response> Handle(
            Create<Aggregate>Command request,
            CancellationToken cancellationToken)
        {
            var entity = factory
                .WithName(request.Name)
                .WithMeasure(request.Measure.Value, request.Measure.Unit)
                .Build();

            await repository.Save(entity, cancellationToken);

            return new Create<Aggregate>Response(entity.Id);
        }
    }
}
```

```csharp
using FluentValidation;

public class Create<Aggregate>CommandValidator : AbstractValidator<Create<Aggregate>Command>
{
    public Create<Aggregate>CommandValidator()
        => Include(new <Aggregate>CommandValidator());
}
```

```csharp
public class Create<Aggregate>Response
{
    internal Create<Aggregate>Response(Guid id) => Id = id;

    public Guid Id { get; }
}
```

### `<Aggregate>s/Commands/Update/Update<Aggregate>Command.cs`

Load, mutate through the aggregate's own methods, save. `NotFoundException` for a missing row — not `Result.Failure`.

```csharp
using MediatR;

public class Update<Aggregate>Command : <Aggregate>Command, IRequest<Result>
{
    public Guid Id { get; set; }

    public class Update<Aggregate>CommandHandler : IRequestHandler<Update<Aggregate>Command, Result>
    {
        private readonly I<Aggregate>DomainRepository repository;

        public Update<Aggregate>CommandHandler(I<Aggregate>DomainRepository repository)
            => this.repository = repository;

        public async Task<Result> Handle(
            Update<Aggregate>Command request,
            CancellationToken cancellationToken)
        {
            var entity = await repository.Find(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(<Aggregate>), request.Id);

            entity
                .UpdateName(request.Name)
                .UpdateMeasure(request.Measure.Value, request.Measure.Unit);

            await repository.Save(entity, cancellationToken);

            return Result.Success;
        }
    }
}
```

### `<Aggregate>s/Commands/Delete/Delete<Aggregate>Command.cs`

```csharp
using MediatR;

public class Delete<Aggregate>Command : EntityCommand, IRequest<Result>
{
    public class Delete<Aggregate>CommandHandler : IRequestHandler<Delete<Aggregate>Command, Result>
    {
        private readonly I<Aggregate>DomainRepository repository;

        public Delete<Aggregate>CommandHandler(I<Aggregate>DomainRepository repository)
            => this.repository = repository;

        public async Task<Result> Handle(
            Delete<Aggregate>Command request,
            CancellationToken cancellationToken)
        {
            await repository.Delete(request.Id, cancellationToken);
            return Result.Success;
        }
    }
}
```

### `<Aggregate>s/Queries/` — read side

The read side never goes through the domain repository. That's the whole reason for the split.

```csharp
using AutoMapper;

public class <Aggregate>Response : <Aggregate>Model
{
    public Guid Id { get; set; }

    public override void Mapping(Profile mapper)
        => mapper.CreateMap<<Aggregate>, <Aggregate>Response>()
            .IncludeBase<<Aggregate>, <Aggregate>Model>();
}
```

```csharp
public interface I<Aggregate>QueryRepository : IQueryRepository<<Aggregate>>
{
    Task<<Aggregate>Response> GetDetailsById(Guid id, CancellationToken cancellationToken = default);
    Task<List<<Aggregate>ListItem>> GetAll(CancellationToken cancellationToken = default);
}
```

```csharp
using MediatR;

public class <Aggregate>DetailsQuery : EntityCommand, IRequest<<Aggregate>Response>
{
    public class <Aggregate>DetailsQueryHandler
        : IRequestHandler<<Aggregate>DetailsQuery, <Aggregate>Response>
    {
        private readonly I<Aggregate>QueryRepository repository;

        public <Aggregate>DetailsQueryHandler(I<Aggregate>QueryRepository repository)
            => this.repository = repository;

        public async Task<<Aggregate>Response> Handle(
            <Aggregate>DetailsQuery request,
            CancellationToken cancellationToken)
            => await repository.GetDetailsById(request.Id, cancellationToken);
    }
}
```

### `<Ctx>ApplicationConfiguration.cs`

```csharp
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class <Ctx>ApplicationConfiguration
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    public static IServiceCollection Add<Ctx>Application(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddCommonApplication(configuration, Assembly);
}
```

MediatR handlers, AutoMapper profiles, and `IEventHandler<T>` implementations are all found by scan.

### `Handlers/<Event>Handler.cs` *(inbound from another context)*

```csharp
public class <Event>Handler : IEventHandler<<Event>>
{
    private readonly I<Aggregate>DomainRepository repository;

    public <Event>Handler(I<Aggregate>DomainRepository repository)
        => this.repository = repository;

    public Task Handle(<Event> domainEvent)
        => repository.RecordSomething(domainEvent.SomeId);
}
```

Writes only *this* context's aggregates — see violation 2.9.

---

## 3. Infrastructure

### `Persistence/<Ctx>DbContext.cs`

```csharp
using System.Reflection;
using Microsoft.EntityFrameworkCore;

internal class <Ctx>DbContext : BaseDbContext<<Ctx>DbContext>
{
    public <Ctx>DbContext(
        DbContextOptions<<Ctx>DbContext> options,
        IEventDispatcher eventDispatcher)
        : base(options, eventDispatcher)
    {
    }

    public DbSet<<Aggregate>> <Aggregate>s { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

Inheriting `BaseDbContext` is what makes domain events dispatch after the write. A plain `DbContext` silently drops every raised event.

### `Configurations/<Aggregate>Configuration.cs`

All persistence shaping lives here — this is what keeps EF out of the model. Value objects map with `OwnsOne`, lengths come from the same constants the domain guards against.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class <Aggregate>Configuration : IEntityTypeConfiguration<<Aggregate>>
{
    public void Configure(EntityTypeBuilder<<Aggregate>> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(<Aggregate>ModelConstants.<Aggregate>.MaxNameLength);

        builder.OwnsOne(e => e.Measure, m =>
        {
            m.WithOwner();

            m.Property(v => v.Value)
                .IsRequired()
                .HasPrecision(38, 15)
                .HasColumnName("MeasureValue");

            m.Property(v => v.Unit)
                .IsRequired()
                .HasMaxLength(<Aggregate>ModelConstants.<Vo>.MaxUnitLength)
                .HasColumnName("MeasureUnit");
        });
    }
}
```

### `Repositories/<Aggregate>Repository.cs`

One `internal` class implementing both interfaces. `IQueryable` never leaves it; reads project straight to the response type so shaping happens in SQL.

```csharp
using AutoMapper;
using Microsoft.EntityFrameworkCore;

internal class <Aggregate>Repository : DataRepository<<Ctx>DbContext, <Aggregate>>,
    I<Aggregate>DomainRepository,
    I<Aggregate>QueryRepository
{
    private readonly IMapper mapper;

    public <Aggregate>Repository(<Ctx>DbContext db, IMapper mapper)
        : base(db)
        => this.mapper = mapper;

    public async Task<<Aggregate>?> Find(Guid id, CancellationToken cancellationToken = default)
        => await All().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<<Aggregate>Response> GetDetailsById(
        Guid id,
        CancellationToken cancellationToken = default)
        => await mapper
               .ProjectTo<<Aggregate>Response>(AllAsNoTracking().Where(e => e.Id == id))
               .FirstOrDefaultAsync(cancellationToken)
           ?? throw new NotFoundException(nameof(<Aggregate>), id);

    public async Task<List<<Aggregate>ListItem>> GetAll(CancellationToken cancellationToken = default)
        => await mapper
            .ProjectTo<<Aggregate>ListItem>(AllAsNoTracking())
            .ToListAsync(cancellationToken);

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Find(id, cancellationToken)
            ?? throw new NotFoundException(nameof(<Aggregate>), id);

        Data.Set<<Aggregate>>().Remove(entity);

        await Data.SaveChangesAsync(cancellationToken);
    }
}
```

### `Persistence/<Ctx>DbInitializer.cs` and `InfrastructureConfiguration.cs`

```csharp
internal class <Ctx>DbInitializer : DbInitializer
{
    public <Ctx>DbInitializer(<Ctx>DbContext db)
        : base(db, new List<IInitialData> { new <Aggregate>Data() })
    {
    }
}
```

```csharp
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class InfrastructureConfiguration
{
    public static IServiceCollection Add<Ctx>Infrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDBStorage<<Ctx>DbContext>(configuration, Assembly.GetExecutingAssembly())
            .AddTransient<IDbInitializer, <Ctx>DbInitializer>();
}
```

### `HttpServices/<OtherCtx>HttpService.cs` *(only for a cross-context read)*

```csharp
using System.Net.Http.Json;

public sealed class <OtherCtx>HttpService : I<OtherCtx>HttpService
{
    private readonly HttpClient client;

    public <OtherCtx>HttpService(HttpClient client) => this.client = client;

    public async Task<<Type>Response?> GetById(string id)
        => await client.GetFromJsonAsync<<Type>Response>($"api/<Resource>/GetById/{id}");
}
```

`<Type>Response` is the **consumer's own** DTO from `<Ctx>.Application/Contracts/<OtherCtx>/`, never the producer's type.

---

## 4. Web

### `Features/<Aggregate>sController.cs`

Bind and `Send`. Nothing else — no repository, no mapping, no business `if`.

```csharp
using Microsoft.AspNetCore.Mvc;

public class <Aggregate>sController : ApiController
{
    [HttpGet]
    [Route(Id)]
    public async Task<ActionResult<<Aggregate>Response>> GetById([FromRoute] <Aggregate>DetailsQuery query)
        => await Send(query);

    [HttpPost]
    public async Task<ActionResult<Create<Aggregate>Response>> Create(Create<Aggregate>Command command)
        => await Send(command);

    [HttpPut]
    [Route(Id)]
    public async Task<ActionResult> Update([FromRoute] Guid id, Update<Aggregate>Command command)
    {
        command.Id = id;                    // Update<Aggregate>Command carries its own Id, so the
        return await Send(command);         // EntityCommand.SetId extension does not apply here
    }

    [HttpDelete]
    [Route(Id)]
    public async Task<ActionResult> Delete([FromRoute] Delete<Aggregate>Command command)
        => await Send(command);
}
```

### `WebConfiguration.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

public static class WebConfiguration
{
    public static IServiceCollection Add<Ctx>WebComponents(this IServiceCollection services)
        => services.AddWebComponents(typeof(<Ctx>ApplicationConfiguration));
}
```

---

## 5. Registration

In `ProjectStartup/Program.cs`, four chained lines in dependency order — and add `<Ctx>.Infrastructure` + `<Ctx>.Web` to `ProjectStartup.csproj`:

```csharp
builder.Services
    .Add<Ctx>Domain()
    .Add<Ctx>Application(builder.Configuration)
    .Add<Ctx>Infrastructure(builder.Configuration)
    .Add<Ctx>WebComponents();
```

---

## 6. Tests

Test projects reference exactly one layer, so a test that needs a database is telling you the layer boundary slipped.

**Domain** — invariants, with no infrastructure at all. If this needs a `DbContext`, the model has an infrastructure dependency.

```csharp
[Fact]
public void Factory_Throws_WhenMeasureMissing()
{
    var factory = new <Aggregate>Factory().WithName("Widget");

    var act = () => factory.Build();

    act.Should().Throw<InvalidOperationException>();
}

[Fact]
public void UpdateName_Rejects_TooShortName()
{
    var entity = Valid<Aggregate>();

    var act = () => entity.UpdateName("a");

    act.Should().Throw<ArgumentException>();
}
```

**Application** — validators and handlers against fakes.

```csharp
public class <Aggregate>CommandValidatorTests
{
    private readonly <Aggregate>CommandValidator validator = new();

    [Fact]
    public void Validator_RejectsEmptyName_WithSpecificError()
    {
        var command = ValidCommand();
        command.Name = string.Empty;

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name is required.");
    }

    private static <Aggregate>Command ValidCommand() => new()
    {
        Name = "Widget",
        Measure = new MeasureRequest(1.5m, "kg"),
    };
}
```

**Architecture** — one project for the whole solution. See `arch-tests.md`.

---

## Minimum viable slice

Don't scaffold the full set by reflex. A single create endpoint is nine files, and everything below is genuinely load-bearing:

```
<Ctx>.Domain/Models/<Aggregate>s/ModelConstants.cs
<Ctx>.Domain/Models/<Aggregate>s/<Aggregate>.cs
<Ctx>.Domain/Factories/I<Aggregate>Factory.cs
<Ctx>.Domain/Factories/<Aggregate>Factory.cs
<Ctx>.Domain/Repositories/I<Aggregate>DomainRepository.cs
<Ctx>.Application/<Aggregate>s/Common/<Aggregate>Model.cs
<Ctx>.Application/<Aggregate>s/Commands/Create/Create<Aggregate>Command.cs
<Ctx>.Application/<Aggregate>s/Commands/Create/Create<Aggregate>Response.cs
<Ctx>.Infrastructure/Configurations/<Aggregate>Configuration.cs
<Ctx>.Infrastructure/Repositories/<Aggregate>Repository.cs
<Ctx>.Web/Features/<Aggregate>sController.cs
```

Add the query repository when there is a read, the `Commands/Common/` pair when a second command shares rules, the event when another context needs to react, and `Contracts/<OtherCtx>/` when a read genuinely crosses a boundary. Creating those before there is a second caller is speculative structure — the kind of over-abstraction that makes people conclude Clean Architecture is bureaucracy.
