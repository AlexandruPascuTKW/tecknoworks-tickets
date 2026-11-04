using Admission.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Admission.Web;

public static class WebConfiguration
{
    public static IServiceCollection AddAdmissionWebComponents(this IServiceCollection services)
        => services.AddWebComponents(typeof(AdmissionApplicationConfiguration));
}
