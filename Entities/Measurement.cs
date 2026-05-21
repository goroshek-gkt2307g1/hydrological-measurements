using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class Measurement
{
    public long MeasurementsId { get; set; }

    public decimal? WaterLevel { get; set; }

    public decimal? WaterConsumption { get; set; }

    public decimal? WaterTemperature { get; set; }

    public decimal? WaterTransparency { get; set; }

    public string? IcePhenomena { get; set; }

    public decimal? Area { get; set; }

    public int SurveyLineIdFk { get; set; }

    public int ProjectIdFk { get; set; }

    public DateTime MeasuredAt { get; set; }

    public decimal? Width { get; set; }

    public int EquipmentIdFk { get; set; }

    public int PersonIdFk { get; set; }

    public string? Comment { get; set; }

    public virtual Equipment EquipmentIdFkNavigation { get; set; } = null!;

    public virtual Person PersonIdFkNavigation { get; set; } = null!;

    public virtual Project ProjectIdFkNavigation { get; set; } = null!;

    public virtual SurveyLine SurveyLineIdFkNavigation { get; set; } = null!;
}
