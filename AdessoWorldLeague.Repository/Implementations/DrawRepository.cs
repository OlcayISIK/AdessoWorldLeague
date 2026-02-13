using AdessoWorldLeague.Core.Enums;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Mongo.Context;
using AdessoWorldLeague.Repository.Interfaces;
using MongoDB.Driver;

namespace AdessoWorldLeague.Repository.Implementations;

public class DrawRepository : GenericRepository<DrawDocument>, IDrawRepository
{
    public DrawRepository(IMongoContext context) : base(context, "draws") { }

    public async Task<List<DrawDocument>> GetAllOrderedByDateAsync()
    {
        var filter = Builders<DrawDocument>.Filter.Eq(x => x.Status, RecordStatus.Active);
        return await Collection.Find(filter)
            .SortByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}
