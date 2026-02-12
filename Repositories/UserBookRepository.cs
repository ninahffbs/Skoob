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

    

}