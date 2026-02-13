namespace AdessoWorldLeague.Dto.Draw;

public class DrawResponse
{
    public string DrawerFirstName { get; set; } = null!;
    public string DrawerLastName { get; set; } = null!;
    public int GroupCount { get; set; }
    public List<GroupDto> Groups { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
