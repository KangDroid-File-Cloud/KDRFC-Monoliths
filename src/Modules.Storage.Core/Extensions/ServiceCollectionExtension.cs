using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Storage.Core.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddStorageCore(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(Assembly.GetExecutingAssembly());

        return serviceCollection;
    }
}