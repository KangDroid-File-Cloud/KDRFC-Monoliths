using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Account.Infrastructure.Persistence;

namespace Modules.Account.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static void MigrateAccountModuleDatabase(this WebApplication webApplication)
    {
        using var scope = webApplication.Services.CreateScope();
        var options = scope.ServiceProvider.GetService<DbContextOptions<AccountDbContext>>();
        using var databaseContext = new AccountDbContext(options);
        databaseContext.Database.Migrate();
    }
}