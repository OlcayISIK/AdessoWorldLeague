using MongoDB.Driver;

namespace AdessoWorldLeague.Mongo.Context;

public interface IMongoContext
{
    IMongoCollection<T> GetCollection<T>(string name);
}
