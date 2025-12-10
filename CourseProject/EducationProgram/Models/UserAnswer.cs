using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class UserAnswer
{
    public int UserId { get; set; }

    public int AnswerId { get; set; }

    public int AttemptNumber { get; set; }

    public virtual Answer Answer { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
