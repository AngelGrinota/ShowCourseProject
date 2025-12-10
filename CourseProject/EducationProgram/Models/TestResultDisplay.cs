using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class TestResultDisplay
{
    public int UserId { get; set; }

    public string UserLogin { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string? UserGroup { get; set; }

    public int TestId { get; set; }

    public string TestName { get; set; } = null!;

    public int AttemptNumber { get; set; }

    public int Score { get; set; }

    public decimal? MaxScore { get; set; }

    public decimal? Percent { get; set; }

    public int Grade { get; set; }

    public DateTime PassedAt { get; set; }
}
