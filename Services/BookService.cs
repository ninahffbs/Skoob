using Skoob.DTOs;
using Skoob.Interfaces;
using Skoob.Repositories;
using Skoob.Models;
namespace Skoob.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly int _pageSize;

    public BookService(IBookRepository bookRepository, IConfiguration configuration)
    {
        _bookRepository = bookRepository;
        _pageSize = configuration.GetValue<int>("Pagination:UsersPageSize");
    }

    private List<BookDTO> MapToDTO(List<Book> books)
    {
        return books.Select(b => new BookDTO
        {
            Id = b.Id,
            Title = b.Title,
            Pages = b.PagesNumber,
            Synopsis = b.Synopsis,
            PublishedDate = b.PublishingYear,
            AuthorName = b.Author?.Name, // Uso do null-conditional para evitar erro
            Genres = b.Genres.Select(g => g.Name).ToList()
        }).ToList();
    }

    public List<BookDTO> GetAllBooks(int page)
    {
        if (page <= 0) page = 1;
        var books = _bookRepository.GetAllBooks(page, _pageSize);
        return MapToDTO(books);
    }

    public List<BookDTO> FilterBookByGenre(string searchedGenre, int page)
    {
        if (page <= 0) page = 1;
        if (string.IsNullOrWhiteSpace(searchedGenre) || searchedGenre.Length < 3)
            throw new ArgumentException("O gênero buscado deve ter pelo menos 3 caracteres.");

        var books = _bookRepository.GetBooksByGenre(searchedGenre, page, _pageSize);
        return MapToDTO(books);
    }

    public List<BookDTO> FilterBookByTitle(string searchedTitle, int page)
    {
        if (page <= 0) page = 1;
        if (string.IsNullOrWhiteSpace(searchedTitle) || searchedTitle.Length < 3)
            throw new ArgumentException("O título deve ter pelo menos 3 caracteres.");

        var books = _bookRepository.GetBooksByTitle(searchedTitle, page, _pageSize);
        return MapToDTO(books);
    }

    public List<BookDTO> FilterBookByAuthor(string searchedAuthor, int page)
    {
        if (page <= 0) page = 1;
        if (string.IsNullOrWhiteSpace(searchedAuthor) || searchedAuthor.Length < 3)
            throw new ArgumentException("O nome do autor deve ter pelo menos 3 caracteres.");

        var books = _bookRepository.GetBooksByAuthor(searchedAuthor, page, _pageSize);
        return MapToDTO(books);
    }
}