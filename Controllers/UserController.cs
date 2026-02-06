using Microsoft.AspNetCore.Mvc;
using Skoob.DTOs;
using Skoob.Interfaces;

namespace Skoob.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet(Name = "GetAllUsers")]
    public ActionResult<List<UserResponseDTO>> Get()
    {
    var users = _service.GetUsers();
        return Ok(users); 
    }

    [HttpPost("Create")]
    public ActionResult<UserResponseDTO> Create(CreateUserDTO dto)
    {
        try
        {
            var user = _service.CreateUser(dto);
            return Ok(user); 
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
}