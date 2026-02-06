using System;
using System.Collections.Generic;

namespace Skoob.Models;

public partial class Userbook
{
    public Guid UserId { get; set; }

    public Guid BookId { get; set; }

    public int? PagesRead { get; set; }

    public short? Rating { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? FinishDate { get; set; }

    public string? Review { get; set; }

    public Guid? StatusId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Statusbook? Status { get; set; }

    public virtual Mainuser User { get; set; } = null!;
}
