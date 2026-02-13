using AdessoWorldLeague.Core.Entities;

namespace AdessoWorldLeague.Data.Documents;

public class TeamDocument : BaseDocument
{
    public string Name { get; set; } = null!;
    public string Country { get; set; } = null!;
}
