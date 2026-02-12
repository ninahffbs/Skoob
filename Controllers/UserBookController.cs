using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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

    [HttpGet("user/{userId}")]
    public ActionResult<List<UserbookResponseDTO>> GetUserBooks(Guid userId)
    {
        try
        {
            var books = _userBookService.GetUserBooks(userId);
            return Ok(books);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("user/{userId}")]
    public ActionResult<UserbookResponseDTO> AddBookToUser(Guid userId, [FromBody] AddBooksUserDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var addedBook = _userBookService.AddBookUser(userId, dto);
            return Ok(addedBook);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("user/{userId}/book/{bookId}")]
    public ActionResult Delete(Guid userId, Guid bookId)
    {
        try
        {
            _userBookService.RemoveUserBook(userId, bookId); 
            return Ok(new { message = "Livro deletado com sucesso do usuário" });
        }   
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    
}