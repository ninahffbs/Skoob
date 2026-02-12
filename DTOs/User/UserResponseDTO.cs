namespace Skoob.DTOs;

public class UserResponseDTO
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public int TotalBooks { get; set; }
    public int BooksRead { get; set; }
}

public class UpdateUserNameRequest
{
    public string UserName { get; set; }
}