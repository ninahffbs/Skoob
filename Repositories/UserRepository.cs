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
    public List<Mainuser> SelectUsers()
    {
        List<Mainuser> mainUsers = _context.Mainusers.ToList();
        return mainUsers;
    }

    public Mainuser? GetUserById(Guid id)
    {
        Mainuser? foundUser = _context.Mainusers.Find(id);
        return foundUser;
    }

    public void UpdateUserName(Mainuser user)
    {
        _context.Mainusers.Update(user);
        _context.SaveChanges();
    }

    public Mainuser CreateUser(Mainuser user)
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