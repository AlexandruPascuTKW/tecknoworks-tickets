using Microsoft.Extensions.DependencyInjection;
using Sales.Application;

namespace Sales.Web;

public static class WebConfiguration
{
    public static IServiceCollection AddSalesWebComponents(this IServiceCollection services)
        => services.AddWebComponents(typeof(SalesApplicationConfiguration));
}
