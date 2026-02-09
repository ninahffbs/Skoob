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

    [HttpGet("all", Name = "GetAllUsers")]
    public ActionResult<List<UserResponseDTO>> GetAll()
    {
        var users = _service.GetUsers();
        return Ok(users);
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public ActionResult<UserResponseDTO> GetById(Guid id)
    {
        var user = _service.GetUserById(id);

        if (user == null)
            return NotFound(new { error = $"Usuário com ID {id} não encontrado" });

        return Ok(user);
    }

    [HttpGet("profile/{username}")]
    public ActionResult<UserResponseDTO> GetProfile(string username)
    {
        var user = _service.GetByUserName(username);
        
        if (user == null)
            return NotFound(new { error = $"Usuário '{username}' não encontrado" });
        
        return Ok(user);
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
    
    [HttpPost("create")]
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

    [HttpDelete("delete/{id}")]
    public ActionResult Delete(Guid id)
    {
        var deleted = _service.DeleteUser(id);

        if (!deleted)
            return NotFound(new { error = $"Usuário com ID {id} não encontrado" });

        return Ok(new { message = "Usuário deletado com sucesso" });
    }
}