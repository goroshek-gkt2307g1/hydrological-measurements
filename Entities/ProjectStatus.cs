using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class ProjectStatus
{
    public int ProjectStatusId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
