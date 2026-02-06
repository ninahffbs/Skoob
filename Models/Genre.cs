using System;
using System.Collections.Generic;

namespace Skoob.Models;

public partial class Genre
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
