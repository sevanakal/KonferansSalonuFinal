using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Passwordresettoken
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Token { get; set; } = null!;

    public DateTime Expirationdate { get; set; }

    public bool Isused { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User User { get; set; } = null!;
}
