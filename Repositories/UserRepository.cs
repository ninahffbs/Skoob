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
}