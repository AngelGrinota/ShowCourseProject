using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class Paragraph
{
    public int ParagraphId { get; set; }

    public int ThemeId { get; set; }

    public string ParagraphTitle { get; set; } = null!;

    public string Path { get; set; } = null!;

    public virtual Theme Theme { get; set; } = null!;
}
