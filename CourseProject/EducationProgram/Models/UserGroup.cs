using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class UserGroup
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
