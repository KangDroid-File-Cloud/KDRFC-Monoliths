using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Models;
using Modules.Storage.Infrastructure.Persistence;

namespace Modules.Storage.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageInfrastructure(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddSingleton<MongoContext>();
        collection.AddSingleton<IGridFsRepository<BlobFile>, GridFsFileRepository<BlobFile>>();

        return collection;
    }
}