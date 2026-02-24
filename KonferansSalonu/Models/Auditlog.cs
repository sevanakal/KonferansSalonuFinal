using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonferansSalonu.Models;

public partial class Auditlog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Userid { get; set; }

    public string? Action { get; set; }

    public string? Details { get; set; }

    public DateTime? Createdat { get; set; }
}
