using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
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

        // Initialize Swagger
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "KDRFC Server",
                Description = "KDRFC Main Server"
            });

            // Include Swagger XML Documentation.
            var includedList = new List<string>
            {
                "ApiHost.xml",
                "Modules.Account.Core.xml",
                "Modules.Account.xml"
            };

            foreach (var eachList in includedList)
            {
                var path = Path.Combine(AppContext.BaseDirectory, eachList);
                if (Path.Exists(path)) options.IncludeXmlComments(path);
            }
        });
        serviceCollection.AddSwaggerGenNewtonsoftSupport();
        return serviceCollection;
    }
}