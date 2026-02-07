using Microsoft.EntityFrameworkCore;
using Skoob.Interfaces;
using Skoob.Models;
using Npgsql;

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

    public string UpdateUserName(Guid id, string newName)
    {
        Mainuser? foundUser = _context.Mainusers.Find(id);
        if (foundUser == null)
        {
            return "user_not_found";
        }
        foundUser.UserName = newName;
        try
        {
            _context.SaveChanges();
            return "okay";
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505")
            {
                return "username_exists";
            }
            throw;
        }
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
}