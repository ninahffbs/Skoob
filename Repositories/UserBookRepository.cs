using Microsoft.EntityFrameworkCore;
using Skoob.Interfaces;
using Skoob.Models;

namespace Skoob.Repositories;

public class UserbookRepository : IUserbookRepository
{
    private readonly PostgresContext _context;

    public UserbookRepository(PostgresContext context)
    {
        _context = context;
    }

    public Userbook AddBookToUser(Userbook userbook)
    {
        _context.Userbooks.Add(userbook);
        _context.SaveChanges();
        return userbook;
    }

    public List<Userbook> GetUserbooksByUserId(Guid userId)
    {
        return [.. _context.Userbooks
            .AsNoTracking()
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Genres)
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.StartDate)];
    }

    public List<Userbook> GetUserBooksByTitle(Guid userId, string title)
    {
        return _context.Userbooks
            .AsNoTracking()
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Genres)
            .Where(ub => ub.UserId == userId && EF.Functions.ILike(ub.Book.Title, $"%{title}%"))
            .ToList();
    }

    public List<Userbook> GetUserBooksByGenre(Guid userId, string genre)
    {
        return _context.Userbooks
            .AsNoTracking()
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Genres)
            .Where(ub => ub.UserId == userId && 
                         ub.Book.Genres.Any(g => EF.Functions.ILike(g.Name, $"%{genre}%")))
            .ToList();
    }

    public List<Userbook> GetUserBooksByAuthor(Guid userId, string author)
    {
        return _context.Userbooks
            .AsNoTracking()
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Genres)
            .Where(ub => ub.UserId == userId && 
                         ub.Book.Author != null && 
                         EF.Functions.ILike(ub.Book.Author.Name, $"%{author}%"))
            .ToList();
    }

    public bool BookExists(Guid bookId) => _context.Books.Any(b => b.Id == bookId);
    
    public bool UserHasBook(Guid userId, Guid bookId) =>
        _context.Userbooks.Any(ub => ub.UserId == userId && ub.BookId == bookId);

    public Userbook? GetUserbook(Guid userId, Guid bookId)
    {
        return _context.Userbooks
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Genres)
            .FirstOrDefault(ub => ub.UserId == userId && ub.BookId == bookId);
    }

    public Userbook? GetUserBookById(Guid userBookId)
    {
        return _context.Userbooks.FirstOrDefault(ub => ub.BookId == userBookId);
    }

    public bool DeleteUserBook(Guid userId, Guid bookId)
    {
        var userbook = _context.Userbooks.FirstOrDefault(ub => ub.UserId == userId && ub.BookId == bookId);
        if (userbook == null) return false;

        _context.Userbooks.Remove(userbook);
        _context.SaveChanges();
        return true;
    }

    public void Update(Userbook userbook)
    {
        _context.Userbooks.Update(userbook);
        _context.SaveChanges();
    }
}