using Microsoft.AspNetCore.Mvc;
using Skoob.DTOs;
using Skoob.Enums;
using Skoob.Interfaces;
using System.Text;

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

    [HttpGet("user/{userId}/filter/title")]
    public IActionResult FilterUserBookByTitle(Guid userId, [FromQuery] string searchedTitle)
    {
        try
        {
            var result = _userBookService.FilterUserBookByTitle(userId, searchedTitle);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/filter/genre")]
    public IActionResult FilterUserBookByGenre(Guid userId, [FromQuery] string searchedGenre)
    {
        try
        {
            var result = _userBookService.FilterUserBookByGenre(userId, searchedGenre);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpGet("{userId}/annual-report/{year}")]
    public IActionResult GenerateAnnualReportFile(Guid userId, int year)
    {
        var report = _userBookService.GenerateAnnualReport(userId, year);

        string reportsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Reports");

        if (!Directory.Exists(reportsFolder))
        {
            Directory.CreateDirectory(reportsFolder);
        }

        string fileName = $"AnnualReadingReport_{userId}_{year}.txt";
        string fullPath = Path.Combine(reportsFolder, fileName);

        var content = new StringBuilder();

        content.AppendLine("======================================");
        content.AppendLine($"Relatório anual de leitura de {report.UserName} - {year} :D");
        content.AppendLine("======================================");
        content.AppendLine($"Data de geração: {DateTime.Now}");
        content.AppendLine();
        content.AppendLine($"Está conosco desde: {report.MemberSince}");
        content.AppendLine($"Ou seja, {report.TimeOnPlatform} de muitas leituras");
        content.AppendLine();
        content.AppendLine($"Total de livros lidos: {report.TotalRead}");
        content.AppendLine($"Total de livros em leitura: {report.TotalReading}");
        content.AppendLine($"Total de livros na lista 'Quero Ler': {report.TotalWantToRead}");
        content.AppendLine();
        content.AppendLine($"Total de páginas lidas: {report.TotalPagesRead}");
        content.AppendLine($"Estimativa de horas lendo: {report.EstimatedReadingHours} horas");
        content.AppendLine();
        content.AppendLine($"Média de avaliação: {report.AverageRating}");
        content.AppendLine($"Gênero favorito: {report.FavoriteGenre}");
        content.AppendLine("======================================");

        System.IO.File.WriteAllText(fullPath, content.ToString());

        return Ok($"Relatório gerado com sucesso em: {fullPath}");
    }
    
    [HttpGet("user/{userId}/filter/author")]
    public IActionResult FilterUserBookByAuthor(Guid userId, [FromQuery] string searchedAuthor)
    {
        try
        {
            var result = _userBookService.FilterUserBookByAuthor(userId, searchedAuthor);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("user/{userId}/book/{bookId}/status")]
    public IActionResult ChangeStatus(Guid userId, Guid bookId, [FromBody] UpdateStatusDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            _userBookService.UpdateStatus(userId, bookId, dto.Status);
            return Ok(new { message = "Status atualizado com sucesso!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("user/{userId}/book/{bookId}/review")]
    public IActionResult AddReview(Guid userId, Guid bookId, [FromBody] UpdateReviewDTO dto)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        try
        {
            _userBookService.UpdateReview(userId, bookId, dto.ReviewText);
            return Ok(new { message = "Resenha atualizada com sucesso!" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { error = ex.Message });
        }
    } 
}