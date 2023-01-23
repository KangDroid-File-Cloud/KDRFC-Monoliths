using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Shared.Core.Abstractions;
using Shared.Infrastructure.Controllers;
using Shared.Infrastructure.Filters;
using Shared.Infrastructure.Persistence;
using StackExchange.Redis;

namespace Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection serviceCollection,
                                                             IConfiguration configuration)
    {
        serviceCollection.AddControllers(a => a.Filters.Add<GlobalExceptionFilter>())
                         .AddNewtonsoftJson()
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
                "Modules.Account.xml",
                "Modules.Storage.xml",
                "Modules.Storage.Core.xml"
            };

            foreach (var eachList in includedList)
            {
                var path = Path.Combine(AppContext.BaseDirectory, eachList);
                if (File.Exists(path))
                {
                    options.IncludeXmlComments(path);
                }
            }
        });
        serviceCollection.AddSwaggerGenNewtonsoftSupport();

        // Add Redis Cache
        serviceCollection.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("CacheConnection")));
        serviceCollection.AddSingleton<ICacheService, CacheService>();

        // Add Metrics
        serviceCollection.AddOpenTelemetry()
                         .WithMetrics(builder =>
                         {
                             builder
                                 .AddMeter("KDRFC")
                                 .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("KDRFC"))
                                 .AddRuntimeInstrumentation()
                                 .AddHttpClientInstrumentation()
                                 .AddAspNetCoreInstrumentation()
                                 .AddPrometheusExporter();

                             if (Convert.ToBoolean(configuration["EnableConsoleMetricsExporter"]))
                             {
                                 builder.AddConsoleExporter();
                             }
                         })
                         .StartWithHost();

        // Add IHttpClientFactory
        serviceCollection.AddHttpClient();

        return serviceCollection;
    }
}