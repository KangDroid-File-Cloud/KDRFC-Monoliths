using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Account.Core.Abstractions;
using Modules.Account.Infrastructure.Persistence;

namespace Modules.Account.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountInfrastructure(this IServiceCollection serviceCollection,
                                                              IConfiguration configuration)
    {
        serviceCollection.AddDbContext<AccountDbContext>(option =>
        {
            option.UseSqlServer(configuration.GetConnectionString("DatabaseConnection"));
            option.EnableDetailedErrors();
            option.EnableSensitiveDataLogging();
        });
        serviceCollection.AddScoped<IAccountDbContext>(provider => provider.GetService<AccountDbContext>());

        return serviceCollection;
    }
}