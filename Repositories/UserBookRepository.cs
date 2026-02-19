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
        return _context.Userbooks
            .AsNoTracking()
            .Include(ub => ub.Book)
                .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
                .ThenInclude(b => b.Genres)
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.StartDate)
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
    public void UpdateReadPages(Userbook userbook)
    {
        _context.Userbooks.Update(userbook);
        _context.SaveChanges();
    }
    public void AddRating(Userbook userbook)
    {
        _context.Userbooks.Update(userbook);
        _context.SaveChanges();
    }

    public void UpdateStatus(Userbook userbook)
    {
        _context.Userbooks.Update(userbook);
        _context.SaveChanges();
    }
    
}