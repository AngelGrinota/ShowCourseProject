using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class Test
{
    public int TestId { get; set; }

    public int ThemeId { get; set; }

    public string TestName { get; set; } = null!;

    public int MaxAttempts { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    public virtual Theme Theme { get; set; } = null!;
}
