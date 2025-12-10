using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class Section
{
    public int SectionId { get; set; }

    public string SectionTitle { get; set; } = null!;

    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();
}
