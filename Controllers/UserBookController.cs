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

    [HttpPatch("user/{userId}/book/{bookId}/pages")]
    public IActionResult UpdateReadPages(Guid userId, Guid bookId, [FromBody] UpdateReadPagesDTO dto)
    {
        try
        {
            _userBookService.UpdateReadPages(userId, bookId, dto.PagesRead);
            return Ok(new { message = "Páginas atualizadas com sucesso!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("user/{userId}/book/{bookId}/rating")]
    public IActionResult AddRating(Guid userId, Guid bookId, [FromBody] AddRatingDTO dto)
    {
        try
        {
            _userBookService.AddRating(userId, bookId, dto.Rating!.Value);
            return Ok(new { message = "Avaliação atualizada com sucesso!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("books")]
    public ActionResult<List<BookDTO>> GetAllBooks([FromQuery] int page = 1)
    {
        var books = _userBookService.GetAllBooks(page);
        return Ok(books);
    }

    [HttpGet("user/{userId}/book/filter")]
    public IActionResult FilterByTitle(Guid userId, [FromQuery] string searchedTitle)
    {
        try
        {
            var result = _userBookService.FilterByTitle(userId, searchedTitle);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}