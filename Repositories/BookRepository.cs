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

    public Book GetBookById(Guid bookId)
    {
        return _context.Books.FirstOrDefault(b => b.Id == bookId) ?? throw new ArgumentException("Livro não encontrado no catálogo");
    }
   
}