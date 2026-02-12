using Microsoft.AspNetCore.Mvc;
using Skoob.DTOs;
using Skoob.Interfaces;

namespace Skoob.Controllers;

[ApiController]
[Route("[controller]")]
public class UserBookController : ControllerBase
{
    private IUserServiceBook _userBookService; 

    public UserBookController(IUserServiceBook userBookService)
    {
        _userBookService = userBookService;
    }

    
}