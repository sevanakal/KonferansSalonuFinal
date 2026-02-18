using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Culture
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
