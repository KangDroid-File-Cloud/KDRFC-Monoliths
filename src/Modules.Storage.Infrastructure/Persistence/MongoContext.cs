using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Modules.Storage.Infrastructure.Persistence;

public class MongoContext
{
    private readonly IMongoClient _mongoClient;
    public readonly IMongoDatabase MongoDatabase;

    public MongoContext(IConfiguration configuration)
    {
        // Setup MongoDB Naming
        var camelCase = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register("CamelCase", camelCase, a => true);

        // Setup Client/Database
        _mongoClient = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
        MongoDatabase = _mongoClient.GetDatabase(configuration["MongoDb:DatabaseName"]);
    }
}