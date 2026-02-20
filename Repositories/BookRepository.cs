using Microsoft.EntityFrameworkCore;

using Skoob.Interfaces;

using Skoob.Models;

namespace Skoob.Repositories;
public class BookRepository : IBookRepository
{
    private readonly PostgresContext _context;

    public BookRepository(PostgresContext context)
    {
        _context = context;
    }

    public List<Book> GetAllBooks(int page, int pageSize)
    {
        return _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Book> GetBooksByGenre(string genre, int page, int pageSize)
    {
        return _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .Where(b => b.Genres.Any(g => EF.Functions.ILike(g.Name, $"%{genre}%")))
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Book> GetBooksByTitle(string title, int page, int pageSize)
    {
        return _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .Where(b => EF.Functions.ILike(b.Title, $"%{title}%"))
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Book> GetBooksByAuthor(string author, int page, int pageSize)
    {
        return _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .Where(b => b.Author != null && EF.Functions.ILike(b.Author.Name, $"%{author}%"))
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Book GetBookById(Guid bookId)
    {
        return _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .FirstOrDefault(b => b.Id == bookId) ?? throw new ArgumentException("Livro não encontrado");
    }
}