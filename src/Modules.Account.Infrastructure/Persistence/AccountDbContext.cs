using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Models.Data;
using Shared.Infrastructure.Persistence;

namespace Modules.Account.Infrastructure.Persistence;

public class AccountDbContext : ModuleDatabaseContext, IAccountDbContext
{
    protected override string Schema => "Account";

    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Core.Models.Data.Account> Accounts { get; set; }
    public DbSet<Credential> Credentials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Core.Models.Data.Account>()
                    .HasKey(a => a.Id);
        modelBuilder.Entity<Core.Models.Data.Account>()
                    .HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Core.Models.Data.Account>()
                    .HasIndex(a => new
                    {
                        a.Id,
                        a.IsDeleted
                    });

        modelBuilder.Entity<Credential>()
                    .HasKey(a =>
                        new
                        {
                            a.AuthenticationProvider,
                            a.ProviderId
                        });
        base.OnModelCreating(modelBuilder);
    }
}