using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Languagekey
{
    public int Id { get; set; }

    public string Keyname { get; set; } = null!;

    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
}
