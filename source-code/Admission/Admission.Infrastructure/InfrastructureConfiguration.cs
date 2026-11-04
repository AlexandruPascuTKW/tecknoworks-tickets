using System.Reflection;
using Admission.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admission.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddAdmissionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDBStorage<AdmissionDbContext>(configuration, Assembly.GetExecutingAssembly())
            .AddTransient<IDbInitializer, AdmissionDbInitializer>();
}
