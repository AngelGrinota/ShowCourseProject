using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class Leaderboard
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public decimal? TotalScore { get; set; }

    public ulong Rank { get; set; }
}
