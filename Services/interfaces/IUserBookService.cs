using Skoob.DTOs;

namespace Skoob.Interfaces; 

public interface IUserServiceBook
{
    public UserbookResponseDTO AddBookUser(Guid userId, AddBooksUserDTO dto);
    public List<UserbookResponseDTO> GetUserBooks(Guid userId);
    public void RemoveUserBook(Guid userId, Guid bookId);
} 