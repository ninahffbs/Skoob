using System;
using System.Collections.Generic;

namespace Skoob.Models;

public partial class Book
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public int PagesNumber { get; set; }

    public string? Synopsis { get; set; }

    public int PublishingYear { get; set; }

    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
