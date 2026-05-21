using System;
using System.Collections.Generic;

namespace Гидрологические_измерения.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string Client { get; set; } = null!;

    public int ContractorIdFk { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Purpose { get; set; }

    public string? ElevationSystem { get; set; }

    public int StatusIdFk { get; set; }

    public virtual Person ContractorIdFkNavigation { get; set; } = null!;

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    public virtual ProjectStatus StatusIdFkNavigation { get; set; } = null!;
}
