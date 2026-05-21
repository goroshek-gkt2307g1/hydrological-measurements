using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class PersonRole
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
