using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Seatgroup
{
    public int Id { get; set; }

    public int Sectionid { get; set; }

    public string Name { get; set; } = null!;

    public string Color { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual Section Section { get; set; } = null!;
}
