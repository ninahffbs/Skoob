using Skoob.Models; 

namespace Skoob.Interfaces; 

public interface IUserbookRepository
{
    public Userbook AddBookToUser(Userbook userbook);
    public List<Userbook> GetUserbooksByUserId(Guid userId);
    bool BookExists(Guid bookId); 
    bool UserHasBook(Guid userId, Guid bookId);
    public Userbook? GetUserbook(Guid userId, Guid bookId);
    public Book GetBookById(Guid bookId);
    public Userbook? GetUserBookById(Guid userBookId);
    public bool DeleteUserBook(Guid userId, Guid bookId);
}