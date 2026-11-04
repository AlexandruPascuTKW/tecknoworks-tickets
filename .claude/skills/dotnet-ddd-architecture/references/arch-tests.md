# Architecture Tests — making the rules automatic

Every rule in `violations.md` that a machine can check should be a failing test rather than a review comment. Code review catches boundary violations inconsistently and only when someone senior is looking; a red build catches them every time, on the branch, before the reference has spread to thirty files.

The rule of thumb: **the second time you write the same architecture finding in a review, write the test instead.** Then say so in the review — "this is the arch test that would have caught it" is worth more than the finding.

## Contents

- [Setup](#setup)
- [Tier 1 — context isolation](#tier-1--context-isolation)
- [Tier 2 — layer dependencies](#tier-2--layer-dependencies)
- [Tier 3 — shared kernel discipline](#tier-3--shared-kernel-discipline)
- [Tier 4 — encapsulation](#tier-4--encapsulation)
- [Tier 5 — conventions](#tier-5--conventions)
- [What tests can't catch](#what-tests-cant-catch)
- [Adoption on an existing solution](#adoption-on-an-existing-solution)

---

## Setup

One test project for the whole solution, referencing every context's four projects plus `Common.*`. This project is the single legitimate exception to the "no project sees two contexts" rule — it exists precisely to check that nobody else does.

```
tests/ArchitectureTests/
  ArchitectureTests.csproj          # references every context project + Common.*
  ArchitectureReferences.cs         # assembly + namespace constants
  ContextIsolationTests.cs          # Tier 1 — the ones that matter most
  LayerDependencyTests.cs           # Tier 2
  SharedKernelTests.cs              # Tier 3
  EncapsulationTests.cs             # Tier 4
  ConventionTests.cs                # Tier 5
```

`NetArchTest.Rules` + xUnit + FluentAssertions. (`ArchUnitNET` works equally well; the rules below translate directly.)

Because the template's projects use the global namespace, filter by **assembly** rather than namespace — `Types.InAssembly(<Ctx>DomainAssembly)`, not `ResideInNamespace("...")`. If your solution does declare namespaces, either axis works.

```csharp
public static class ArchitectureReferences
{
    public static readonly Assembly ProductCatalogDomain = typeof(Product).Assembly;
    public static readonly Assembly ProductCatalogApplication = typeof(CreateProductCommand).Assembly;
    public static readonly Assembly ProductCatalogInfrastructure = typeof(ProductRepository).Assembly;
    public static readonly Assembly ProductCatalogWeb = typeof(ProductsController).Assembly;

    // …one block per context…

    public static Assembly[] ContextAssemblies(string context) => context switch
    {
        "ProductCatalog"  => [ProductCatalogDomain, ProductCatalogApplication,
                              ProductCatalogInfrastructure, ProductCatalogWeb],
        "OrderManagement" => [OrderManagementDomain, OrderManagementApplication,
                              OrderManagementInfrastructure, OrderManagementWeb],
        _ => throw new ArgumentOutOfRangeException(nameof(context)),
    };

    public static string Failures(TestResult result)
        => string.Join(Environment.NewLine, result.FailingTypeNames ?? []);
}
```

`Failures` matters more than it looks: an arch test that fails with `Assert.True(false)` teaches nobody. Always assert with the failing type names in the message.

## Tier 1 — context isolation

The most valuable tests in the suite. If you write only one tier, write this one.

```csharp
[Theory]
[InlineData("ProductCatalog", "OrderManagement")]
[InlineData("ProductCatalog", "Statistics")]
[InlineData("OrderManagement", "ProductCatalog")]
[InlineData("OrderManagement", "Statistics")]
[InlineData("Statistics", "ProductCatalog")]
[InlineData("Statistics", "OrderManagement")]
public void Context_ShouldNotDependOn_AnyOtherContext(string context, string other)
{
    var otherTypes = ContextAssemblies(other)
        .SelectMany(a => a.GetTypes())
        .Select(t => t.FullName!)
        .ToArray();

    var result = Types
        .InAssemblies(ContextAssemblies(context))
        .Should()
        .NotHaveDependencyOnAny(otherTypes)
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        $"{context} must not depend on {other}. Offenders:{Environment.NewLine}{Failures(result)}");
}
```

Keep the `InlineData` pairs generated from a single context list so adding a context automatically adds its tests. A context missing from this matrix is unprotected, which is why it's on the new-context checklist in `layer-map.md`.

Belt and braces at the project level, because it catches the violation earlier and reads more clearly in a failure:

```csharp
[Fact]
public void No_Csproj_References_Another_Context()
{
    var contexts = new[] { "ProductCatalog", "OrderManagement", "Statistics", "Identity" };
    var offenders = new List<string>();

    foreach (var csproj in Directory.EnumerateFiles(SolutionRoot, "*.csproj", SearchOption.AllDirectories))
    {
        var owner = contexts.FirstOrDefault(c => csproj.Contains($"{Path.DirectorySeparatorChar}{c}{Path.DirectorySeparatorChar}"));
        if (owner is null) continue;                    // Common, ProjectStartup, tests

        var text = File.ReadAllText(csproj);
        offenders.AddRange(
            from foreign in contexts.Where(c => c != owner)
            where text.Contains($"\\{foreign}\\") || text.Contains($"/{foreign}/")
            select $"{Path.GetFileName(csproj)} → {foreign}");
    }

    offenders.Should().BeEmpty();
}
```

## Tier 2 — layer dependencies

```csharp
[Theory]
[MemberData(nameof(AllContexts))]
public void Domain_ShouldOnlyDependOn_CommonDomain(string context)
{
    var result = Types
        .InAssembly(DomainAssembly(context))
        .Should()
        .NotHaveDependencyOnAny(
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "MediatR",
            "FluentValidation",
            "AutoMapper",
            "System.Net.Http")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Application_ShouldNotDependOn_InfrastructureOrWeb(string context)
{
    var result = Types
        .InAssembly(ApplicationAssembly(context))
        .Should()
        .NotHaveDependencyOnAny(
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Web_ShouldNotDependOn_Infrastructure(string context)
{
    var infrastructureTypes = InfrastructureAssembly(context)
        .GetTypes().Select(t => t.FullName!).ToArray();

    var result = Types
        .InAssembly(WebAssembly(context))
        .Should()
        .NotHaveDependencyOnAny(infrastructureTypes)
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}
```

The `Web → Infrastructure` test is the one people are surprised by, and the one that keeps DI registration in `ProjectStartup` where it belongs.

Also worth asserting, since attributes on entities are the classic slow leak:

```csharp
[Theory]
[MemberData(nameof(AllContexts))]
public void Domain_Entities_ShouldNotCarry_PersistenceAttributes(string context)
{
    var offenders = DomainAssembly(context).GetTypes()
        .Where(t => typeof(Entity).IsAssignableFrom(t))
        .SelectMany(t => t.GetProperties().Concat<MemberInfo>([t]))
        .Where(m => m.GetCustomAttributes()
            .Any(a => a.GetType().Namespace?.StartsWith("System.ComponentModel.DataAnnotations") == true
                   || a.GetType().Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true))
        .Select(m => $"{m.DeclaringType?.Name}.{m.Name}");

    offenders.Should().BeEmpty();
}
```

## Tier 3 — shared kernel discipline

`Common` is where a clean reference graph hides a coupled solution, so these two are high value despite being cheap.

```csharp
[Fact]
public void CommonDomain_ShouldContain_NoAggregates()
{
    var result = Types
        .InAssembly(CommonDomainAssembly)
        .That().ImplementInterface(typeof(IAggregateRoot))
        .Should().NotExist()
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        $"business aggregates belong to a context, not the shared kernel:{Environment.NewLine}{Failures(result)}");
}

[Fact]
public void CrossContext_Events_ShouldCarry_PrimitivesOnly()
{
    var allowed = new[]
    {
        typeof(Guid), typeof(string), typeof(int), typeof(long), typeof(decimal),
        typeof(double), typeof(bool), typeof(DateTime), typeof(DateTimeOffset),
    };

    var offenders = CommonDomainAssembly.GetTypes()
        .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsInterface)
        .SelectMany(t => t.GetProperties())
        .Where(p =>
        {
            var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            return !allowed.Contains(type) && !type.IsEnum;
        })
        .Select(p => $"{p.DeclaringType!.Name}.{p.Name} : {p.PropertyType.Name}");

    offenders.Should().BeEmpty(
        "an event carrying a domain object re-couples the contexts through the shared kernel");
}
```

## Tier 4 — encapsulation

```csharp
[Theory]
[MemberData(nameof(AllContexts))]
public void Aggregate_Constructors_ShouldNotBe_Public(string context)
{
    var offenders = DomainAssembly(context).GetTypes()
        .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
        .Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
        .Select(t => t.Name);

    offenders.Should().BeEmpty("aggregates are created through their factory");
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Entities_ShouldNotHave_PublicSetters(string context)
{
    var offenders = DomainAssembly(context).GetTypes()
        .Where(t => typeof(Entity).IsAssignableFrom(t))
        .SelectMany(t => t.GetProperties())
        .Where(p => p.SetMethod?.IsPublic == true)
        .Select(p => $"{p.DeclaringType!.Name}.{p.Name}");

    offenders.Should().BeEmpty("state changes go through intention-named methods that validate");
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Infrastructure_Implementations_ShouldBe_Internal(string context)
{
    var result = Types
        .InAssembly(InfrastructureAssembly(context))
        .That().Inherit(typeof(DbContext))
        .Or().ImplementInterface(typeof(IEntityTypeConfiguration<>))
        .Or().HaveNameEndingWith("Repository")
        .Or().HaveNameEndingWith("DbInitializer")
        .Should().NotBePublic()
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}

[Theory]
[MemberData(nameof(AllContexts))]
public void QueryRepositories_ShouldNotExpose_IQueryable(string context)
{
    var offenders = ApplicationAssembly(context).GetTypes()
        .Concat(DomainAssembly(context).GetTypes())
        .Where(t => t.IsInterface && t.Name.EndsWith("Repository"))
        .SelectMany(t => t.GetMethods())
        .Where(m => m.ReturnType.Name.Contains("IQueryable"))
        .Select(m => $"{m.DeclaringType!.Name}.{m.Name}");

    offenders.Should().BeEmpty("deferred queries must not escape Infrastructure");
}
```

## Tier 5 — conventions

Cheap, and what keeps the higher tiers legible — a misnamed file makes a real violation invisible.

```csharp
[Theory]
[MemberData(nameof(AllContexts))]
public void Handlers_ShouldBe_NestedInTheirRequest(string context)
{
    var offenders = ApplicationAssembly(context).GetTypes()
        .Where(t => t.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
        .Where(t => !t.IsNested)
        .Select(t => t.Name);

    offenders.Should().BeEmpty("a handler lives inside the command or query it handles");
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Commands_ShouldEndWith_Command(string context)
{
    var result = Types
        .InAssembly(ApplicationAssembly(context))
        .That().ImplementInterface(typeof(IRequest<>))
        .And().DoNotHaveNameEndingWith("Query")
        .Should().HaveNameEndingWith("Command")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Controllers_ShouldInherit_ApiController(string context)
{
    var result = Types
        .InAssembly(WebAssembly(context))
        .That().HaveNameEndingWith("Controller")
        .Should().Inherit(typeof(ApiController))
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}

[Theory]
[MemberData(nameof(AllContexts))]
public void Controllers_ShouldNotDependOn_Repositories(string context)
{
    var repositories = DomainAssembly(context).GetTypes()
        .Concat(ApplicationAssembly(context).GetTypes())
        .Where(t => t.IsInterface && t.Name.EndsWith("Repository"))
        .Select(t => t.FullName!).ToArray();

    var result = Types
        .InAssembly(WebAssembly(context))
        .That().HaveNameEndingWith("Controller")
        .Should().NotHaveDependencyOnAny(repositories)
        .GetResult();

    result.IsSuccessful.Should().BeTrue(Failures(result));
}
```

## What tests can't catch

Say this plainly when someone asks whether arch tests make review unnecessary. They don't. These remain human judgement, and they are exactly the findings that matter most in a mature codebase:

- **Whether the boundary is in the right place.** Tests enforce the boundary you declared, including a wrong one — and enforce it harder every month.
- **An anemic model.** No public setter, no violation flagged, and all the rules still sitting in handlers.
- **Persistence quietly bending the model** — an FK property, a flattened value object, a shape that exists for the ORM. Every reference is legal.
- **A cross-context need routed the wrong way** — HTTP where an event belonged, an N+1 on a hot path.
- **Whether an abstraction earns its keep.** An interface with one implementation and no substitution point passes every test.
- **Whether a `Result` should have been an exception**, or the reverse.

## Adoption on an existing solution

A suite that fails on day one gets `[Skip]`-ed and then deleted. Land it so it's green from the first commit:

1. **Add Tier 1 first, alone.** Context isolation is the highest-value rule and usually the closest to already passing.
2. **For each pre-existing violation, add an explicit, named exemption** — not a weakened rule. `.Where(t => t != typeof(LegacyOrderSync))` with a comment naming the ticket. A visible list of six exemptions shrinks; a loosened rule never tightens.
3. **Wire it into CI** as part of `dotnet test`, so the build breaks on the branch rather than in review.
4. **Add tiers one at a time**, each with its own exemption list, in the order above — isolation, layers, shared kernel, encapsulation, conventions. Value descends in exactly that order, so stopping early is a legitimate choice.
5. **Never delete a failing test to go green.** Either fix the code or add a named exemption with a reason. The exemption list is the honest architecture debt register, and it is more useful than a passing suite that no longer means anything.
