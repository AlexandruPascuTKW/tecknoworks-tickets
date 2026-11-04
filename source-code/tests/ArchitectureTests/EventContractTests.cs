using FluentAssertions;
using Xunit;

/// <summary>
/// Cross-context event contracts live in Common.Domain and must carry primitives only (architecture.md,
/// ADR-005). A domain object in a payload would re-couple the contexts through the shared kernel.
/// </summary>
public class EventContractTests
{
    private static readonly Type[] Allowed =
    {
        typeof(string), typeof(Guid), typeof(decimal), typeof(DateTime), typeof(DateOnly), typeof(TimeOnly),
        typeof(bool), typeof(int), typeof(long),
    };

    private static IEnumerable<Type> Contracts
        => typeof(IDomainEvent).Assembly
            .GetTypes()
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .Where(t => t != typeof(DeferredDomainEvent));

    [Fact]
    public void There_Are_Event_Contracts_To_Check()
        => Contracts.Should().NotBeEmpty();

    [Fact]
    public void Event_Contracts_Carry_Primitives_Only()
    {
        var offenders = new List<string>();

        foreach (var contract in Contracts)
        {
            Check(contract, contract.Name, offenders, depth: 0);
        }

        offenders.Should().BeEmpty("an event contract may hold only primitives and arrays of records of primitives");
    }

    private static void Check(Type type, string path, List<string> offenders, int depth)
    {
        foreach (var property in type.GetProperties().Where(p => p.DeclaringType == type))
        {
            var propertyType = property.PropertyType;
            var name = $"{path}.{property.Name}";

            if (propertyType.IsArray)
            {
                propertyType = propertyType.GetElementType()!;
            }

            if (Allowed.Contains(propertyType) || propertyType.IsEnum)
            {
                continue;
            }

            // A nested record of primitives, one level down.
            if (depth == 0 && propertyType.IsSealed && propertyType.Namespace is null
                && !typeof(Entity).IsAssignableFrom(propertyType) && !typeof(ValueObject).IsAssignableFrom(propertyType))
            {
                Check(propertyType, name, offenders, depth + 1);
                continue;
            }

            offenders.Add($"{name} is a {propertyType.Name}");
        }
    }
}
