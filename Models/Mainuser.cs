using System;
using System.Collections.Generic;

namespace Skoob.Models;

public partial class Mainuser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();
}
