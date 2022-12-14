using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Services;

namespace Shared.Core.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSharedCoreServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IJwtService, JwtService>();
        return serviceCollection;
    }
}