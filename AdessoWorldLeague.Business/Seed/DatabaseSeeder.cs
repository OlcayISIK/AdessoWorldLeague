using Microsoft.Extensions.Logging;
using AdessoWorldLeague.Core.Constants;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Repository.Interfaces;

namespace AdessoWorldLeague.Business.Seed;

public class DatabaseSeeder
{
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ITeamRepository teamRepository, ILogger<DatabaseSeeder> logger)
    {
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var count = await _teamRepository.CountAsync();
        if (count > 0)
            return;

        var teams = TeamData.AllTeams.Select(t => new TeamDocument
        {
            Name = t.Name,
            Country = t.Country.ToString()
        }).ToList();

        await _teamRepository.SeedAsync(teams);
        _logger.LogInformation("{Count} takim basariyla veritabanina eklendi", teams.Count);
    }
}
