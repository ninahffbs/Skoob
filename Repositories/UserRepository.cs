using Microsoft.EntityFrameworkCore;
using Skoob.Interfaces;
using Skoob.Models;

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