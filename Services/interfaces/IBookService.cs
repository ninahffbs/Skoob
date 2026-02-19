using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IBookService
{
    public List<BookDTO> FilterBookByGenre(string searchedGenre, int page);
    public List<BookDTO> FilterBookByTitle(string searchedTitle, int page);
    public List<BookDTO> FilterBookByAuthor(string searchedAuthor, int page);
    public List<BookDTO> GetAllBooks(int page);
}