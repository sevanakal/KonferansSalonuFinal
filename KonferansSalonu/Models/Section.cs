using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Section
{
    public int Id { get; set; }

    public int Roomid { get; set; }

    public string Name { get; set; } = null!;

    public string? Color { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Seatgroup> Seatgroups { get; set; } = new List<Seatgroup>();
}
