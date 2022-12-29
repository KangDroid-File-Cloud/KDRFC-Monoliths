using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Infrastructure.Extensions;

namespace Modules.Storage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageModule(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddStorageCore();
        serviceCollection.AddStorageInfrastructure(configuration);

        return serviceCollection;
    }
}