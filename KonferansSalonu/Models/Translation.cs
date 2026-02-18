using System;
using System.Collections.Generic;

namespace KonferansSalonu.Models;

public partial class Translation
{
    public int Id { get; set; }

    public int Cultureid { get; set; }

    public int Keyid { get; set; }

    public string Value { get; set; } = null!;

    public virtual Culture Culture { get; set; } = null!;

    public virtual Languagekey Key { get; set; } = null!;
}
