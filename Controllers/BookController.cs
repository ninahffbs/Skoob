using Microsoft.AspNetCore.Mvc;
using Skoob.Interfaces;
using Skoob.DTOs;
using Skoob.Repositories;

namespace Skoob.Controllers;

[ApiController]
[Route("[controller]")]
public class BookController : ControllerBase
{
    private IBookService _BookService;

    public BookController(IBookService BookService)
    {
        _BookService = BookService;
    }

    [HttpGet("books")]
    public ActionResult<List<BookDTO>> GetAllBooks([FromQuery] int page = 1)
    {
        var books = _BookService.GetAllBooks(page);
        return Ok(books);
    }

    [HttpGet("filter/title")]
    public IActionResult FilterBookByTitle([FromQuery] string searchedTitle, [FromQuery] int page = 1)
    {
        var books = _BookService.FilterBookByTitle(searchedTitle, page);
        return Ok(books);
    }

     [HttpGet("filter/genre")]
    public IActionResult FilterBookByGenre([FromQuery] string searchedGenre, [FromQuery] int page = 1)
    {
        try
        {
            var result = _BookService.FilterBookByGenre(searchedGenre, page);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}