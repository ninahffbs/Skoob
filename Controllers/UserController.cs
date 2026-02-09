using System.Net;
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

    [HttpPatch("{id}/username", Name = "UpdateUserName")]
    public IActionResult UpdateUserName([FromRoute] Guid id, [FromBody] UpdateUserNameRequest request)
    {
        try
        {
            _service.UpdateUserName(id, request.UserName);
            return Ok(new { message = "Nome de usuário atualizado com sucesso" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
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

    [HttpPatch("{id}/password", Name = "UpdatePassword")]
    public IActionResult UpdatePassword([FromRoute] Guid id, [FromBody] UpdatePasswordDTO dto)
    {
        try
        {
            _service.UpdatePassword(id, dto);
            return Ok(new { message = "Senha do usuário atualizada com sucesso!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }
}