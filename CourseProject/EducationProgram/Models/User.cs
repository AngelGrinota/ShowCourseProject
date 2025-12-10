using System;
using System.Collections.Generic;

namespace EducationProgram.Models;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public int? GroupId { get; set; }

    public string Login { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual UserGroup? Group { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
