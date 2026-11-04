using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Admission.Domain;

public static class DomainConfiguration
{
    public static IServiceCollection AddAdmissionDomain(this IServiceCollection services)
        => services.AddCommonDomain(Assembly.GetExecutingAssembly());
}
