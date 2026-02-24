using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonferansSalonu.Models;

public partial class Translation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Cultureid { get; set; }

    public int Keyid { get; set; }

    public string Value { get; set; } = null!;

    public virtual Culture Culture { get; set; } = null!;

    public virtual Languagekey Key { get; set; } = null!;
}
