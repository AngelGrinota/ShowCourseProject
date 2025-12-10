using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int TestId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int Points { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Test Test { get; set; } = null!;
}
