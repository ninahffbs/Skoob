using Skoob.Models; 

namespace Skoob.Interfaces;

public interface IUserbookRepository
{
    public Userbook AddBookToUser(Userbook userbook);
    public List<Userbook> GetUserbooksByUserId(Guid userId);
    bool BookExists(Guid bookId);
    bool UserHasBook(Guid userId, Guid bookId);
    public Userbook? GetUserbook(Guid userId, Guid bookId);
    public Userbook? GetUserBookById(Guid userBookId);
    public bool DeleteUserBook(Guid userId, Guid bookId);
    public void Update(Userbook userbook);
    public List<Userbook> GetUserBooksByTitle(Guid userId, string title);
    public List<Userbook> GetUserBooksByGenre(Guid userId, string genre);
    public List<Userbook> GetUserBooksByAuthor(Guid userId, string author);
}