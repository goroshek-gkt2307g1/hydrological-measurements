using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class SurveyLine
{
    public int SurveyLineId { get; set; }

    public int HydropostIdFk { get; set; }

    public string Name { get; set; } = null!;

    public decimal? DistanceFromBase { get; set; }

    public virtual Hydropost HydropostIdFkNavigation { get; set; } = null!;

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
