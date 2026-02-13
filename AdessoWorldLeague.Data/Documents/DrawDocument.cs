using AdessoWorldLeague.Core.Entities;

namespace AdessoWorldLeague.Data.Documents;

public class DrawDocument : BaseDocument
{
    public string DrawerFirstName { get; set; } = null!;
    public string DrawerLastName { get; set; } = null!;
    public int GroupCount { get; set; }
    public List<GroupInfo> Groups { get; set; } = new();
}

public class GroupInfo
{
    public string GroupName { get; set; } = null!;
    public List<TeamInfo> Teams { get; set; } = new();
}

public class TeamInfo
{
    public string Name { get; set; } = null!;
    public string Country { get; set; } = null!;
}
