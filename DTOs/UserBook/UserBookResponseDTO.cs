using Skoob.Enums;

namespace Skoob.DTOs;

public class UserbookResponseDTO
{
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = null!;
    public int BookPages { get; set; }
    public string? AuthorName { get; set; }

    public int PagesRead { get; set; }
    public int PercentComplete { get; set; } 

    public StatusBook Status { get; set; }
    public string StatusName { get; set; } = null!;

    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }

    public short? Rating { get; set; }
    public string? Review { get; set; }
}