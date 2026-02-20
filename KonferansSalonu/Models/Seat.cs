using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Seat
{
    public int Id { get; set; }

    public int? Seatgroupid { get; set; }

    public string Label { get; set; } = null!;

    /// <summary>
    /// 1-armchair, 2-chair, 3-table, 4-rectangletable, 5-stage
    /// </summary>
    public string Type { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Rotation { get; set; }

    public int? Status { get; set; }

    public int? Defaultwidth { get; set; }

    public int? Defaultheight { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int? Scalepercentage { get; set; }

    public int Isresize { get; set; }

    public virtual Seatgroup? Seatgroup { get; set; }
}
