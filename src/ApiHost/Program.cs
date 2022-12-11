using Microsoft.OpenApi.Models;
using Modules.Account.Extensions;
using Modules.Account.Infrastructure.Extensions;
using Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add Account Module
builder.Services.AddAccountModule(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
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
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, eachList));
    }
});

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