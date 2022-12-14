using Modules.Account.Extensions;
using Modules.Account.Infrastructure.Extensions;
using Modules.Account.Infrastructure.Persistence;
using Modules.Storage.Extensions;
using Shared.Core.Extensions;
using Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddSharedCoreServices();

// Add Account Module
builder.Services.AddAccountModule(builder.Configuration);

// Add Storage Module
builder.Services.AddStorageModule(builder.Configuration);

// Add Health Check
builder.Services.AddHealthChecks()
       .AddDbContextCheck<AccountDbContext>()
       .AddRedis(builder.Configuration.GetConnectionString("CacheConnection")!);

var app = builder.Build();

// Migrate Account Module Database
app.MigrateAccountModuleDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsTestEnvironment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/healthz");

app.Run();