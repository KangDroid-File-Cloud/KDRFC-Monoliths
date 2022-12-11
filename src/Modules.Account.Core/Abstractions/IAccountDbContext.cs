using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Models.Data;

namespace Modules.Account.Core.Abstractions;

public interface IAccountDbContext
{
    public DbSet<Models.Data.Account> Accounts { get; set; }
    public DbSet<Credential> Credentials { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}