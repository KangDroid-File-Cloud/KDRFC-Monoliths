using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Persistence;

public abstract class ModuleDatabaseContext : DbContext
{
    protected abstract string Schema { get; }

    protected ModuleDatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(Schema)) modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}