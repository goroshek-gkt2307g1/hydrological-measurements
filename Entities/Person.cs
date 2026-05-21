using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class Person
{
    public int PersonId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int RoleIdFk { get; set; }

    public virtual Hydropost? Hydropost { get; set; }

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual PersonRole RoleIdFkNavigation { get; set; } = null!;
}
