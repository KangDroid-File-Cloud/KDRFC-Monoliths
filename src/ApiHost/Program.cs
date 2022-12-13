using Modules.Account.Extensions;
using Modules.Account.Infrastructure.Extensions;
using Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add Account Module
builder.Services.AddAccountModule(builder.Configuration);

var app = builder.Build();

// Migrate Account Module Database
app.MigrateAccountModuleDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();