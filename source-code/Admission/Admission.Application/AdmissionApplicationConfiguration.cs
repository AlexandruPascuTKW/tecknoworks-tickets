using System.Reflection;
using Admission.Application.Common;
using Admission.Application.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admission.Application;

public static class AdmissionApplicationConfiguration
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    public static IServiceCollection AddAdmissionApplication(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddCommonApplication(configuration, Assembly)
            .AddTransient<TeamAccess>()
            .AddTransient<PerformanceSynchronizer>();
}
