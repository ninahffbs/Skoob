using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IUserServiceBook
{
    public UserbookResponseDTO AddBookUser(Guid userId, AddBooksUserDTO dto);
    public List<UserbookResponseDTO> GetUserBooks(Guid userId);
    public void RemoveUserBook(Guid userId, Guid bookId);
    public void UpdateReadPages(Guid userId, Guid bookId, int newPages);
    public void AddRating(Guid userId, Guid bookId, int rating);
    public List<BookDTO> GetAllBooks(int page);
} 