using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skoob.Models;

[Table("statusbooks", Schema = "skoob")]
public partial class Statusbook
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Status { get; set; } = null!;

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();
}
