using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Auditlog
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string? Action { get; set; }

    public string? Details { get; set; }

    public DateTime? Createdat { get; set; }
}
