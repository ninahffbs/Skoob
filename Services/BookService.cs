using Skoob.DTOs;
using Skoob.Interfaces;
using Skoob.Repositories;

namespace Skoob.Services;

public class BookService : IBookService {
    private readonly IBookRepository _bookRepository;
    private readonly int _pageSize;

    public BookService(
        IBookRepository bookRepository,
        IConfiguration configuration)
    {
        _bookRepository = bookRepository;
        _pageSize = configuration.GetValue<int>("Pagination:UsersPageSize");
    }

       public List<BookDTO> GetAllBooks(int page)
    {
        if (page <= 0)
            page = 1;

        var books = _bookRepository.GetAllBooks(page, _pageSize);

        return books.Select(b => new BookDTO
        {
            Id = b.Id,
            Title = b.Title,
            Pages = b.PagesNumber,
            Synopsis = b.Synopsis,
            PublishedDate = b.PublishingYear,
            AuthorName = b.Author?.Name,
            Genres = b.Genres.Select(g => g.Name).ToList()
        }).ToList();
    }

    //FilterBookByGenre
    public List<BookDTO> FilterBookByGenre(string searchedGenre, int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (string.IsNullOrWhiteSpace(searchedGenre) || searchedGenre.Length < 3)
        {
            throw new ArgumentException("O gênero buscado deve ter pelo menos 3 caracteres.");
        }

        var books = _bookRepository.GetAllBooks(page, _pageSize);

        var filteredBooks = books
            .Where(b => b.Genres
                .Any(g => g.Name
                    .Contains(searchedGenre, StringComparison.OrdinalIgnoreCase)))
            .Select(b => new BookDTO
            {
                Id = b.Id,
                Title = b.Title,
                Pages = b.PagesNumber,
                Synopsis = b.Synopsis,
                PublishedDate = b.PublishingYear,
                AuthorName = b.Author.Name,
                Genres = b.Genres
                    .Select(g => g.Name)
                    .ToList()
            })
            .ToList();

        return filteredBooks;
    }

    public List<BookDTO> FilterBookByTitle(string searchedTitle, int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (string.IsNullOrWhiteSpace(searchedTitle) || searchedTitle.Length < 3)
        {
            throw new ArgumentException("O título deve ter pelo menos 3 caracteres.");
        }

        var books = _bookRepository.GetAllBooks(page, _pageSize);

        var filteredBooks = books
            .Where(b => b.Title
                .Contains(searchedTitle, StringComparison.OrdinalIgnoreCase))
            .Select(b => new BookDTO
            {
                Id = b.Id,
                Title = b.Title,
                Pages = b.PagesNumber,
                Synopsis = b.Synopsis,
                PublishedDate = b.PublishingYear,
                AuthorName = b.Author.Name,
                Genres = [.. b.Genres.Select(g => g.Name)]
            })
            .ToList();

        return filteredBooks;
    }

    public List<BookDTO> FilterBookByAuthor(string searchedAuthor, int page)
    {
        if (page <= 0) page = 1;

        if (string.IsNullOrWhiteSpace(searchedAuthor) || searchedAuthor.Length < 3)
        {
            throw new ArgumentException("O nome do autor deve ter pelo menos 3 caracteres.");
        }

        var books = _bookRepository.GetAllBooks(page, _pageSize);

        var filteredBooks = books
            .Where(b => b.Author != null && b.Author.Name 
            .Contains(searchedAuthor, StringComparison.OrdinalIgnoreCase))
            .Select(b => new BookDTO
            {   
                Id = b.Id,
                Title = b.Title,
                Pages = b.PagesNumber,
                Synopsis = b.Synopsis,
                PublishedDate = b.PublishingYear,
                AuthorName = b.Author.Name,
                Genres = [.. b.Genres.Select(g => g.Name)]
            })
            .ToList();

        return filteredBooks;
    }
}