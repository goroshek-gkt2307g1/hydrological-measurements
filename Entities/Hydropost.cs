using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class Hydropost
{
    public int HydropostId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int HydropostTypeIdFk { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longtude { get; set; }

    public decimal ZeroWaterLevel { get; set; }

    public int ManagerIdFk { get; set; }

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual HydropostType HydropostTypeIdFkNavigation { get; set; } = null!;

    public virtual Person ManagerIdFkNavigation { get; set; } = null!;

    public virtual ICollection<SurveyLine> SurveyLines { get; set; } = new List<SurveyLine>();
}
