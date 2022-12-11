using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Account.Core.Extensions;
using Modules.Account.Infrastructure.Extensions;

namespace Modules.Account.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountModule(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddAccountInfrastructure(configuration)
                         .AddAccountCore();
        return serviceCollection;
    }
}