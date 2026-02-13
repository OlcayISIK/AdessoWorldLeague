namespace AdessoWorldLeague.Dto.Draw;

public class GroupDto
{
    public string GroupName { get; set; } = null!;
    public List<TeamDto> Teams { get; set; } = new();
}
