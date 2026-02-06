using System;
using System.Collections.Generic;

namespace Skoob.Models;

public partial class Statusbook
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Status { get; set; } = null!;

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();
}
