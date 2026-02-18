using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Role { get; set; }

    public int? Cultureid { get; set; }

    public virtual Culture? Culture { get; set; }

    public virtual ICollection<Passwordresettoken> Passwordresettokens { get; set; } = new List<Passwordresettoken>();
}
