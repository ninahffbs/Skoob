using Skoob.Models; 

namespace Skoob.Interfaces;

public interface IBookRepository
{
    public List<Book> GetAllBooks(int page, int pageSize);
    public Book GetBookById(Guid bookId);
    List<Book> GetBooksByGenre(string genre, int page, int pageSize);
    List<Book> GetBooksByTitle(string title, int page, int pageSize);
    List<Book> GetBooksByAuthor(string author, int page, int pageSize);
}