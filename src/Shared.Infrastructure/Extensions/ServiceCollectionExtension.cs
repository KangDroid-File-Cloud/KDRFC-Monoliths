using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Controllers;
using Shared.Infrastructure.Filters;

namespace Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection serviceCollection,
                                                             IConfiguration configuration)
    {
        serviceCollection.AddControllers(a => a.Filters.Add<GlobalExceptionFilter>())
                         .ConfigureApplicationPartManager(manager =>
                             manager.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return serviceCollection;
    }
}