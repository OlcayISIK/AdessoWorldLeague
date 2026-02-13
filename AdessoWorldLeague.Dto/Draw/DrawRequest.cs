using System.ComponentModel.DataAnnotations;

namespace AdessoWorldLeague.Dto.Draw;

public class DrawRequest
{
    [Required]
    [Range(4, 8)]
    public int GroupCount { get; set; }
}
