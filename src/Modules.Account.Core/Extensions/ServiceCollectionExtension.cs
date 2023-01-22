using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services;
using Modules.Account.Core.Services.Authentication;

namespace Modules.Account.Core.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddAccountCore(this IServiceCollection services)
    {
        // Register MediatR for this Account Module.
        services.AddMediatR(Assembly.GetExecutingAssembly());

        // Register Authentication Provider.
        services.AddTransient<SelfAuthenticationService>();
        services.AddTransient<GoogleAuthenticationService>();
        services.AddScoped<AuthenticationProviderFactory>(serviceProvider => provider =>
        {
            return provider switch
            {
                AuthenticationProvider.Self => serviceProvider.GetRequiredService<SelfAuthenticationService>(),
                AuthenticationProvider.Google => serviceProvider.GetRequiredService<GoogleAuthenticationService>(),
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unknown Provider")
            };
        });
        return services;
    }
}