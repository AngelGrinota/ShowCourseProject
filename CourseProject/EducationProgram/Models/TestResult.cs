using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class TestResult
{
    public int UserId { get; set; }

    public int TestId { get; set; }

    public int AttemptNumber { get; set; }

    public int Score { get; set; }

    public DateTime PassedAt { get; set; }

    public virtual Test Test { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
