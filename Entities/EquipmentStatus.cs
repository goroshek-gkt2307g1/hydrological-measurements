using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class EquipmentStatus
{
    public int EquipmentStatusId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}
