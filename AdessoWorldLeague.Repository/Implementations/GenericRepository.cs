using System.Linq.Expressions;
using AdessoWorldLeague.Core.Entities;
using AdessoWorldLeague.Core.Enums;
using AdessoWorldLeague.Mongo.Context;
using AdessoWorldLeague.Repository.Interfaces;
using MongoDB.Driver;

namespace AdessoWorldLeague.Repository.Implementations;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseDocument
{
    protected readonly IMongoCollection<T> Collection;

    private static readonly FilterDefinition<T> ActiveFilter =
        Builders<T>.Filter.Eq(x => x.Status, RecordStatus.Active);

    protected GenericRepository(IMongoContext context, string collectionName)
    {
        Collection = context.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id) & ActiveFilter;
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await Collection.Find(ActiveFilter).ToListAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        var combined = Builders<T>.Filter.Where(filter) & ActiveFilter;
        return await Collection.Find(combined).ToListAsync();
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
    {
        var combined = Builders<T>.Filter.Where(filter) & ActiveFilter;
        return await Collection.Find(combined).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        entity.Status = RecordStatus.Active;
        await Collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(string id, T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<T>.Filter.Eq(x => x.Id, id) & ActiveFilter;
        await Collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(string id)
    {
        var update = Builders<T>.Update
            .Set(x => x.Status, RecordStatus.Deleted)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var filter = Builders<T>.Filter.Eq(x => x.Id, id) & ActiveFilter;
        await Collection.UpdateOneAsync(filter, update);
    }

    // DO NOT USE IF IT IS NOT NECESSARY !!!
    public async Task HardDeleteAsync(string id)
    {
        await Collection.DeleteOneAsync(x => x.Id == id);
    }
}
