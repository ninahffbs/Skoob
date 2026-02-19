namespace Skoob.DTOs;

public class AnnualReadingReportDTO
{
    public int Year { get; set; }
    public int TotalRead { get; set; }
    public int TotalReading { get; set; }
    public int TotalWantToRead { get; set; }
    public int TotalPagesRead { get; set; }
    public double EstimatedReadingHours { get; set; }
    public double AverageRating { get; set; }
    public string? FavoriteGenre { get; set; }
}