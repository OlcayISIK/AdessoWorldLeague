using Microsoft.Extensions.Localization;
using AdessoWorldLeague.Business.Interfaces;
using AdessoWorldLeague.Business.Resources;
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
    private readonly IStringLocalizer<Messages> _localizer;

    public DrawService(IDrawRepository drawRepository, ITeamRepository teamRepository, IStringLocalizer<Messages> localizer)
    {
        _drawRepository = drawRepository;
        _teamRepository = teamRepository;
        _localizer = localizer;
    }

    public async Task<OperationResult<DrawResponse>> PerformDrawAsync(DrawOperationRequest request)
    {

        if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            return OperationResult<DrawResponse>.Failure(_localizer["NameRequired"]);

        if (!TeamData.AllowedGroupCounts.Contains(request.GroupCount))
            return OperationResult<DrawResponse>.Failure(_localizer["InvalidGroupCount"]);

        var allTeams = await _teamRepository.GetAllAsync();
        if (allTeams.Count == 0)
            return OperationResult<DrawResponse>.Failure(_localizer["NoTeamsFound"]);

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

        return OperationResult<DrawResponse>.Success(MapToResponse(document), _localizer["DrawCompleted"]);
    }

    public async Task<OperationResult<DrawResponse>> GetDrawByIdAsync(string id)
    {
        var document = await _drawRepository.GetByIdAsync(id);
        if (document is null)
            return OperationResult<DrawResponse>.Failure(_localizer["DrawNotFound"]);

        return OperationResult<DrawResponse>.Success(MapToResponse(document), _localizer["DrawRetrieved"]);
    }

    public async Task<OperationResult<List<DrawResponse>>> GetAllDrawsAsync()
    {
        var documents = await _drawRepository.GetAllOrderedByDateAsync();
        var responses = documents.Select(MapToResponse).ToList();
        return OperationResult<List<DrawResponse>>.Success(responses, _localizer["DrawsRetrieved"]);
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
            Id = document.Id,
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
