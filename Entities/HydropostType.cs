using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class HydropostType
{
    public int HydropostTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Hydropost> Hydroposts { get; set; } = new List<Hydropost>();
}
