using System;

namespace AdessoWorldLeague.Dto.Draw;

public class DrawGroupResult
{
        public string GroupName { get; set; } = null!;
        public List<DrawTeamResult> Teams { get; set; } = new();
}
