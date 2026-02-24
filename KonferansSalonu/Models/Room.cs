using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonferansSalonu.Models;

public partial class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Capacity { get; set; }

    public bool Isdisabled { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
