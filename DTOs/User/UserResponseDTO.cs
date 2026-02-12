namespace Skoob.DTOs;

public class UserResponseDTO
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public int TotalBooks { get; set; }
    public int BooksRead { get; set; }
    public List<UserBookSimpleDTO> Books { get; set; } = new();
}

public class UpdateUserNameRequest
{
    public string UserName { get; set; }
}

public class UserBookSimpleDTO
{
    public string BookTitle { get; set; } = null!;
    public int PagesRead { get; set; }
    public int PercentComplete { get; set; }
    public string Status { get; set; } = null!; 
    public DateTime? StartedAt { get; set; }
}