using Microsoft.Extensions.Options;
using MongoDB.Driver;
using AdessoWorldLeague.Mongo.Settings;

namespace AdessoWorldLeague.Mongo.Context;

public class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
