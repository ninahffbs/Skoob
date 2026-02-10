using Microsoft.EntityFrameworkCore;
using Skoob.Interfaces;
using Skoob.Models;
using Npgsql;
using System.Diagnostics.Tracing;

namespace Skoob.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PostgresContext _context;

    public UserRepository(PostgresContext context)
    {
        _context = context;
    }
    public List<Mainuser> SelectUsers(int page, int pageSize)
    {
        return _context.Mainusers
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    
    public Mainuser? GetById(Guid id)
    {
        Mainuser? foundUser = _context.Mainusers.Find(id);
        return foundUser;
    }

    public Mainuser? GetByName(string username)
    {
        return _context.Mainusers
        .AsNoTracking()
        .Include(u => u.Userbooks)
        .ThenInclude(ub => ub.Book)
        .FirstOrDefault(u => EF.Functions.ILike(u.UserName, username));
    }

    public void UpdateUserName(Mainuser user)
    {
        _context.Mainusers.Update(user);
        _context.SaveChanges();
    }

     public Mainuser Register(Mainuser user)
    {
        _context.Mainusers.Add(user);
        _context.SaveChanges();
        return user;
    }

    public bool UsernameExists(string username)
    {
        return _context.Mainusers
            .Any(u => EF.Functions.ILike(u.UserName, username));
    }

    public bool EmailExists(string email)
    {
        return _context.Mainusers
            .Any(u => EF.Functions.ILike(u.Email, email));
    }

    public void UpdatePassword(Mainuser user)
    {
        _context.Mainusers.Update(user);
        _context.SaveChanges();
    }

     public bool DeleteUser(Guid id)
    {
        var user = _context.Mainusers.Find(id);

        if (user == null)
            return false; 
        _context.Mainusers.Remove(user);
        _context.SaveChanges();
        
        return true; 
    }
}