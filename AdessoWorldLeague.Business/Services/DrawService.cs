using Microsoft.Extensions.Logging;
using AdessoWorldLeague.Business.Interfaces;
using AdessoWorldLeague.Core.Constants;
using AdessoWorldLeague.Core.Results;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Dto.Draw;
using AdessoWorldLeague.Repository.Interfaces;

namespace AdessoWorldLeague.Business.Services;

public class DrawService : IDrawService
{
    private readonly IDrawRepository _drawRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<DrawService> _logger;

    public DrawService(IDrawRepository drawRepository, ITeamRepository teamRepository, ILogger<DrawService> logger)
    {
        _drawRepository = drawRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<OperationResult<DrawResponse>> PerformDrawAsync(DrawRequest request)
    {
        if (!TeamData.AllowedGroupCounts.Contains(request.GroupCount))
            return OperationResult<DrawResponse>.Failure("The number of groups should be 4 or 8.");

        var allTeams = await _teamRepository.GetAllAsync();
        if (allTeams.Count == 0)
            return OperationResult<DrawResponse>.Failure("No team was found in the database.");

        var groups = ExecuteDraw(request.GroupCount, allTeams);

        var document = new DrawDocument
        {
            DrawerFirstName = request.FirstName,
            DrawerLastName = request.LastName,
            GroupCount = request.GroupCount,
            Groups = groups.Select(g => new GroupInfo
            {
                GroupName = g.GroupName,
                Teams = g.Teams.Select(t => new TeamInfo
                {
                    Name = t.Name,
                    Country = t.Country
                }).ToList()
            }).ToList()
        };

        await _drawRepository.CreateAsync(document);

        _logger.LogInformation("The draw has been completed. Drawn by: {FirstName} {LastName}, Group number: {GroupCount}",
            request.FirstName, request.LastName, request.GroupCount);

        return OperationResult<DrawResponse>.Success(MapToResponse(document));
    }

    public async Task<OperationResult<DrawResponse>> GetDrawByIdAsync(string id)
    {
        var document = await _drawRepository.GetByIdAsync(id);
        if (document is null)
            return OperationResult<DrawResponse>.Failure("The draw was not found.");

        return OperationResult<DrawResponse>.Success(MapToResponse(document));
    }

    public async Task<OperationResult<List<DrawResponse>>> GetAllDrawsAsync()
    {
        var documents = await _drawRepository.GetAllOrderedByDateAsync();
        var responses = documents.Select(MapToResponse).ToList();
        return OperationResult<List<DrawResponse>>.Success(responses);
    }

    private static List<DrawGroupResult> ExecuteDraw(int groupCount, List<TeamDocument> allTeams)
    {
        var teamsPerGroup = allTeams.Count / groupCount;
        var groupNames = TeamData.GroupNames.Take(groupCount).ToArray();
        var random = new Random();

        while (true)
        {
            var pool = allTeams.OrderBy(_ => random.Next()).ToList();
            var groups = groupNames.Select(name => new DrawGroupResult { GroupName = name }).ToArray();
            var success = true;

            for (var round = 0; round < teamsPerGroup && success; round++)
            {
                for (var g = 0; g < groupCount && success; g++)
                {
                    var countriesInGroup = groups[g].Teams.Select(t => t.Country).ToHashSet();
                    var validTeams = pool.Where(t => !countriesInGroup.Contains(t.Country)).ToList();

                    if (validTeams.Count == 0)
                    {
                        success = false;
                        break;
                    }

                    var selected = validTeams[random.Next(validTeams.Count)];
                    groups[g].Teams.Add(new DrawTeamResult
                    {
                        Name = selected.Name,
                        Country = selected.Country
                    });
                    pool.Remove(selected);
                }
            }

            if (success)
                return groups.ToList();
        }
    }

    private static DrawResponse MapToResponse(DrawDocument document)
    {
        return new DrawResponse
        {
            DrawerFirstName = document.DrawerFirstName,
            DrawerLastName = document.DrawerLastName,
            GroupCount = document.GroupCount,
            CreatedAt = document.CreatedAt,
            Groups = document.Groups.Select(g => new GroupDto
            {
                GroupName = g.GroupName,
                Teams = g.Teams.Select(t => new TeamDto { Name = t.Name }).ToList()
            }).ToList()
        };
    }
}
