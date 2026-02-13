using AdessoWorldLeague.Data.Documents;

namespace AdessoWorldLeague.Repository.Interfaces;

public interface ITeamRepository
{
    Task<List<TeamDocument>> GetAllAsync();
    Task<List<TeamDocument>> GetByCountryAsync(string country);
    Task<long> CountAsync();
    Task SeedAsync(List<TeamDocument> teams);
}
