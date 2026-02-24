using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonferansSalonu.Models;

public partial class Section
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Roomid { get; set; }

    public string Name { get; set; } = null!;

    public string? Color { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Seatgroup> Seatgroups { get; set; } = new List<Seatgroup>();
}
