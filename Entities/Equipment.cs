using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int EquipmentTypeIdFk { get; set; }

    public DateOnly? LastCalibration { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? HydropostIdFk { get; set; }

    public int EquipmentStatusIdFk { get; set; }

    public virtual EquipmentStatus EquipmentStatusIdFkNavigation { get; set; } = null!;

    public virtual EquipmentType EquipmentTypeIdFkNavigation { get; set; } = null!;

    public virtual Hydropost? HydropostIdFkNavigation { get; set; }

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
