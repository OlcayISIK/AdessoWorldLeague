using Microsoft.Extensions.Localization;
using Moq;
using AdessoWorldLeague.Business.Services;
using AdessoWorldLeague.Business;
using AdessoWorldLeague.Core.Constants;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Dto.Draw;
using AdessoWorldLeague.Repository.Interfaces;

namespace AdessoWorldLeague.Tests;

public class DrawServiceTests
{
    private readonly Mock<IDrawRepository> _drawRepository;
    private readonly Mock<ITeamRepository> _teamRepository;
    private readonly Mock<IStringLocalizer<Messages>> _localizer;
    private readonly DrawService _sut;

    public DrawServiceTests()
    {
        _drawRepository = new Mock<IDrawRepository>();
        _teamRepository = new Mock<ITeamRepository>();
        _localizer = new Mock<IStringLocalizer<Messages>>();

        _localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _sut = new DrawService(_drawRepository.Object, _teamRepository.Object, _localizer.Object);
    }

    [Fact]
    public async Task PerformDrawAsync_TeamCountNotDivisibleByGroupCount_ReturnsFailure()
    {
        var teams = CreateTeams(("Turkiye", 5), ("Almanya", 5));
        _teamRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(teams);

        var request = new DrawOperationRequest
        {
            GroupCount = 4,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.PerformDrawAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("TeamCountNotDivisible", result.Message);
    }

    [Fact]
    public async Task PerformDrawAsync_CountryHasMoreTeamsThanGroups_ReturnsFailure()
    {
        // 5 teams from Turkiye with 4 groups -> impossible to avoid same-country clash
        var teams = CreateTeams(("Turkiye", 5), ("Almanya", 3));
        _teamRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(teams);

        var request = new DrawOperationRequest
        {
            GroupCount = 4,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.PerformDrawAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("TooManyTeamsFromSameCountry", result.Message);
    }

    [Fact]
    public async Task PerformDrawAsync_ValidTeamDistribution_ReturnsSuccess()
    {
        var teams = CreateTeams(
            ("Turkiye", 4), ("Almanya", 4),
            ("Fransa", 4), ("Hollanda", 4),
            ("Portekiz", 4), ("Italya", 4),
            ("Ispanya", 4), ("Belcika", 4)
        );
        _teamRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(teams);
        _drawRepository.Setup(r => r.CreateAsync(It.IsAny<DrawDocument>())).Returns(Task.CompletedTask);

        var request = new DrawOperationRequest
        {
            GroupCount = 4,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.PerformDrawAsync(request);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task PerformDrawAsync_EmptyFirstName_ReturnsFailure()
    {
        var request = new DrawOperationRequest
        {
            GroupCount = 4,
            FirstName = "",
            LastName = "Doe"
        };

        var result = await _sut.PerformDrawAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("NameRequired", result.Message);
    }

    [Fact]
    public async Task PerformDrawAsync_InvalidGroupCount_ReturnsFailure()
    {
        var request = new DrawOperationRequest
        {
            GroupCount = 3,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.PerformDrawAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidGroupCount", result.Message);
    }

    [Fact]
    public async Task PerformDrawAsync_NoTeamsInDatabase_ReturnsFailure()
    {
        _teamRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TeamDocument>());

        var request = new DrawOperationRequest
        {
            GroupCount = 4,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.PerformDrawAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("NoTeamsFound", result.Message);
    }

    // --- GetDrawByIdAsync Tests ---

    [Fact]
    public async Task GetDrawByIdAsync_ExistingId_ReturnsSuccess()
    {
        var document = CreateDrawDocument("64a1b2c3d4e5f6a7b8c9d0e1");
        _drawRepository.Setup(r => r.GetByIdAsync("64a1b2c3d4e5f6a7b8c9d0e1"))
            .ReturnsAsync(document);

        var result = await _sut.GetDrawByIdAsync("64a1b2c3d4e5f6a7b8c9d0e1");

        Assert.True(result.IsSuccess);
        Assert.Equal("DrawRetrieved", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("64a1b2c3d4e5f6a7b8c9d0e1", result.Data.Id);
        Assert.Equal("John", result.Data.DrawerFirstName);
        Assert.Equal("Doe", result.Data.DrawerLastName);
        Assert.Equal(4, result.Data.GroupCount);
    }

    [Fact]
    public async Task GetDrawByIdAsync_NonExistentId_ReturnsFailure()
    {
        _drawRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((DrawDocument?)null);

        var result = await _sut.GetDrawByIdAsync("nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Equal("DrawNotFound", result.Message);
    }

    [Fact]
    public async Task GetDrawByIdAsync_MapsGroupsCorrectly()
    {
        var document = CreateDrawDocument("64a1b2c3d4e5f6a7b8c9d0e1");
        _drawRepository.Setup(r => r.GetByIdAsync("64a1b2c3d4e5f6a7b8c9d0e1"))
            .ReturnsAsync(document);

        var result = await _sut.GetDrawByIdAsync("64a1b2c3d4e5f6a7b8c9d0e1");

        Assert.Equal(2, result.Data!.Groups.Count);
        Assert.Equal("A", result.Data.Groups[0].GroupName);
        Assert.Equal("B", result.Data.Groups[1].GroupName);
        Assert.Equal("Adesso Istanbul", result.Data.Groups[0].Teams[0].Name);
        Assert.Equal("Adesso Berlin", result.Data.Groups[1].Teams[0].Name);
    }

    // --- GetAllDrawsAsync Tests ---

    [Fact]
    public async Task GetAllDrawsAsync_WithDraws_ReturnsAll()
    {
        var documents = new List<DrawDocument>
        {
            CreateDrawDocument("id1"),
            CreateDrawDocument("id2"),
            CreateDrawDocument("id3")
        };
        _drawRepository.Setup(r => r.GetAllOrderedByDateAsync()).ReturnsAsync(documents);

        var result = await _sut.GetAllDrawsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("DrawsRetrieved", result.Message);
        Assert.Equal(3, result.Data!.Count);
    }

    [Fact]
    public async Task GetAllDrawsAsync_Empty_ReturnsEmptyList()
    {
        _drawRepository.Setup(r => r.GetAllOrderedByDateAsync())
            .ReturnsAsync(new List<DrawDocument>());

        var result = await _sut.GetAllDrawsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAllDrawsAsync_MapsAllFieldsCorrectly()
    {
        var doc = CreateDrawDocument("id1");
        _drawRepository.Setup(r => r.GetAllOrderedByDateAsync())
            .ReturnsAsync(new List<DrawDocument> { doc });

        var result = await _sut.GetAllDrawsAsync();

        var response = result.Data![0];
        Assert.Equal("id1", response.Id);
        Assert.Equal("John", response.DrawerFirstName);
        Assert.Equal("Doe", response.DrawerLastName);
        Assert.Equal(4, response.GroupCount);
        Assert.Equal(doc.CreatedAt, response.CreatedAt);
        Assert.Equal(2, response.Groups.Count);
    }

    // --- Helpers ---

    private static List<TeamDocument> CreateTeams(params (string Country, int Count)[] groups)
    {
        var teams = new List<TeamDocument>();
        foreach (var (country, count) in groups)
        {
            for (var i = 0; i < count; i++)
            {
                teams.Add(new TeamDocument
                {
                    Name = $"Adesso {country} {i + 1}",
                    Country = country
                });
            }
        }
        return teams;
    }

    private static DrawDocument CreateDrawDocument(string id)
    {
        return new DrawDocument
        {
            Id = id,
            DrawerFirstName = "John",
            DrawerLastName = "Doe",
            GroupCount = 4,
            Groups = new List<GroupInfo>
            {
                new()
                {
                    GroupName = "A",
                    Teams = new List<TeamInfo>
                    {
                        new() { Name = "Adesso Istanbul", Country = "Turkiye" },
                        new() { Name = "Adesso Paris", Country = "Fransa" }
                    }
                },
                new()
                {
                    GroupName = "B",
                    Teams = new List<TeamInfo>
                    {
                        new() { Name = "Adesso Berlin", Country = "Almanya" },
                        new() { Name = "Adesso Amsterdam", Country = "Hollanda" }
                    }
                }
            }
        };
    }
}
