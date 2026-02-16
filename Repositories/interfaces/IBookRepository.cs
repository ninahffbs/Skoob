using Skoob.Models; 

namespace Skoob.Interfaces;

public interface IBookRepository
{
    public List<Book> GetAllBooks(int page, int pageSize);
    public Book GetBookById(Guid bookId);
}