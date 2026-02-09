using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skoob.Models;

[Table("genre", Schema = "skoob")]
public partial class Genre
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
