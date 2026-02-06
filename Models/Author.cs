using System;
using System.Collections.Generic;

namespace Skoob.Models;

public partial class Author
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = null!;

    public DateOnly Birthdate { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
