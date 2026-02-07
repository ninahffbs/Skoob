using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skoob.Models;

[Table("mainuser", Schema = "skoob")]
public partial class Mainuser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();
}
