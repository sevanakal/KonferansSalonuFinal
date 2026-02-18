using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Capacity { get; set; }

    public bool Isdisabled { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
