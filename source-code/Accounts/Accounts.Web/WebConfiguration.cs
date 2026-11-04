using Accounts.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.Web;

public static class WebConfiguration
{
    public static IServiceCollection AddAccountsWebComponents(this IServiceCollection services)
        => services.AddWebComponents(typeof(AccountsApplicationConfiguration));
}
