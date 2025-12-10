using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class Theme
{
    public int ThemeId { get; set; }

    public int SectionId { get; set; }

    public string ThemeTitle { get; set; } = null!;

    public virtual ICollection<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();

    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
