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

    [HttpPatch("{id}", Name = "UpdateUserName")]
    public IActionResult UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserNameRequest request)
    {
        string result = _service.UpdateUserName(id, request.UserName);
        switch (result)
        {
            case "okay":
                return Ok();
            case "user_not_found":
                return NotFound("Usuário não encontrado");
            case "username_exists":
                return Conflict("Esse nome de usuário já está sendo utilizado.");
            default:
                return StatusCode(500, "Erro inesperado ao atualizar usuário!");
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
}