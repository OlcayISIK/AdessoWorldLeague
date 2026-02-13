using AdessoWorldLeague.Core.Enums;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Mongo.Context;
using AdessoWorldLeague.Repository.Interfaces;
using MongoDB.Driver;

namespace AdessoWorldLeague.Repository.Implementations;

public class TeamRepository : ITeamRepository
{
    private readonly IMongoCollection<TeamDocument> _teams;

    public TeamRepository(IMongoContext context)
    {
        _teams = context.GetCollection<TeamDocument>("teams");
    }

    public async Task<List<TeamDocument>> GetAllAsync()
    {
        var filter = Builders<TeamDocument>.Filter.Eq(x => x.Status, RecordStatus.Active);
        return await _teams.Find(filter).ToListAsync();
    }

    public async Task<List<TeamDocument>> GetByCountryAsync(string country)
    {
        var filter = Builders<TeamDocument>.Filter.Eq(x => x.Country, country)
                     & Builders<TeamDocument>.Filter.Eq(x => x.Status, RecordStatus.Active);
        return await _teams.Find(filter).ToListAsync();
    }

    public async Task<long> CountAsync()
    {
        var filter = Builders<TeamDocument>.Filter.Eq(x => x.Status, RecordStatus.Active);
        return await _teams.CountDocumentsAsync(filter);
    }

    public async Task SeedAsync(List<TeamDocument> teams)
    {
        await _teams.InsertManyAsync(teams);
    }
}
