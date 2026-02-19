using NUnit.Framework;
using Moq;
using Skoob.Controllers;
using Skoob.DTOs;
using Skoob.Enums;
using Skoob.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Skoob.Tests.Controllers;

[TestFixture]
public class UserBookControllerTests
{
    private Mock<IUserServiceBook> _mockUserBookService;
    private UserBookController _controller;

    [SetUp]
    public void Setup()
    {
        _mockUserBookService = new Mock<IUserServiceBook>();
        _controller = new UserBookController(_mockUserBookService.Object);
    }

    [Test]
    public void AddBookToUser_WithValidData_ReturnsOkResult()
    {
        var userId = Guid.NewGuid();
        var dto = new AddBooksUserDTO
        {
            BookId = Guid.NewGuid(),
            Status = StatusBook.Lendo,
            PagesRead = 50,
            StartDate = DateTime.UtcNow
        };
        var expectedResponse = new UserbookResponseDTO
        {
            BookId = dto.BookId,
            Status = StatusBook.Lendo
        };

        _mockUserBookService
            .Setup(s => s.AddBookUser(userId, dto))
            .Returns(expectedResponse);

        var result = _controller.AddBookToUser(userId, dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result).Value, Is.EqualTo(expectedResponse));
        _mockUserBookService.Verify(s => s.AddBookUser(userId, dto), Times.Once);
    }

    [Test]
    public void AddBookToUser_UserNotFound_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var dto = new AddBooksUserDTO { BookId = Guid.NewGuid() };

        _mockUserBookService
            .Setup(s => s.AddBookUser(userId, dto))
            .Throws(new ArgumentException($"Usuário com ID {userId} Não encontrado"));

        var result = _controller.AddBookToUser(userId, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void AddBookToUser_BookNotFound_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dto = new AddBooksUserDTO { BookId = bookId };

        _mockUserBookService
            .Setup(s => s.AddBookUser(userId, dto))
            .Throws(new ArgumentException($"Livro com id {bookId} não encontrado"));

        var result = _controller.AddBookToUser(userId, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void AddBookToUser_UserAlreadyHasBook_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var dto = new AddBooksUserDTO { BookId = Guid.NewGuid() };

        _mockUserBookService
            .Setup(s => s.AddBookUser(userId, dto))
            .Throws(new ArgumentException("Você já tem esse livro adicionado."));

        var result = _controller.AddBookToUser(userId, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetUserBooks_WithValidUserId_ReturnsOkResult()
    {
        var userId = Guid.NewGuid();
        var expectedBooks = new List<UserbookResponseDTO>
        {
            new() { BookId = Guid.NewGuid(), Status = StatusBook.Lendo },
            new() { BookId = Guid.NewGuid(), Status = StatusBook.Lido }
        };

        _mockUserBookService
            .Setup(s => s.GetUserBooks(userId))
            .Returns(expectedBooks);

        var result = _controller.GetUserBooks(userId);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result).Value, Is.EqualTo(expectedBooks));
    }

    [Test]
    public void GetUserBooks_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        _mockUserBookService
            .Setup(s => s.GetUserBooks(userId))
            .Throws(new ArgumentException($"Usuário com {userId} não encontrado"));

        var result = _controller.GetUserBooks(userId);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public void Delete_WithValidData_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var result = _controller.Delete(userId, bookId);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _mockUserBookService.Verify(s => s.RemoveUserBook(userId, bookId), Times.Once);
    }

    [Test]
    public void Delete_WhenServiceThrows_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        _mockUserBookService
            .Setup(s => s.RemoveUserBook(userId, bookId))
            .Throws(new ArgumentException("Livro não encontrado"));

        var result = _controller.Delete(userId, bookId);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public void UpdateReadPages_WithValidData_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var dto = new UpdateReadPagesDTO
        {
            PagesRead = 120
        };

        var result = _controller.UpdateReadPages(userId, bookId, dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _mockUserBookService.Verify(
            s => s.UpdateReadPages(userId, bookId, dto.PagesRead),
            Times.Once);
    }

    [Test]
    public void UpdateReadPages_WhenServiceThrows_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var dto = new UpdateReadPagesDTO
        {
            PagesRead = -10
        };

        _mockUserBookService
            .Setup(s => s.UpdateReadPages(userId, bookId, dto.PagesRead))
            .Throws(new ArgumentException("Páginas inválidas"));

        var result = _controller.UpdateReadPages(userId, bookId, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void AddRating_WithValidData_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var dto = new AddRatingDTO
        {
            Rating = 5
        };

        var result = _controller.AddRating(userId, bookId, dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _mockUserBookService.Verify(
            s => s.AddRating(userId, bookId, dto.Rating!.Value),
            Times.Once);
    }

    [Test]
    public void AddRating_WhenServiceThrows_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var dto = new AddRatingDTO
        {
            Rating = 10
        };

        _mockUserBookService
            .Setup(s => s.AddRating(userId, bookId, dto.Rating!.Value))
            .Throws(new ArgumentException("Nota inválida"));

        var result = _controller.AddRating(userId, bookId, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void FilterUserBookByGenre_WhenServiceThrows_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var genre = "Fantasia";

        _mockUserBookService
            .Setup(s => s.FilterUserBookByGenre(userId, genre))
            .Throws(new ArgumentException("Erro ao filtrar"));

        var result = _controller.FilterUserBookByGenre(userId, genre);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void AddBookToUser_WithInvalidModelState_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var dto = new AddBooksUserDTO();

        _controller.ModelState.AddModelError("BookId", "Required");

        var result = _controller.AddBookToUser(userId, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
    [Test]
    public void FilterUserBookByTitle_WithValidData_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var title = "Harry Potter";

        var expected = new List<UserbookResponseDTO>
        {
            new() { BookId = Guid.NewGuid(), Status = StatusBook.Lendo }
        };

        _mockUserBookService
            .Setup(s => s.FilterUserBookByTitle(userId, title))
            .Returns(expected);

        var result = _controller.FilterUserBookByTitle(userId, title);

        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }
    [Test]
    public void FilterUserBookByGenre_WithValidData_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var genre = "Fantasia";

        var expected = new List<UserbookResponseDTO>
        {
            new() { BookId = Guid.NewGuid(), Status = StatusBook.Lido }
        };

        _mockUserBookService
            .Setup(s => s.FilterUserBookByGenre(userId, genre))
            .Returns(expected);

        var result = _controller.FilterUserBookByGenre(userId, genre);

        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }
    [Test]
    public void GetUserBooks_WhenNoBooks_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        _mockUserBookService
            .Setup(s => s.GetUserBooks(userId))
            .Returns(new List<UserbookResponseDTO>());

        var result = _controller.GetUserBooks(userId);

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(((List<UserbookResponseDTO>)okResult!.Value!).Count, Is.EqualTo(0));
    }

    [Test]
    public void Delete_WithValidData_ReturnsCorrectMessage()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var result = _controller.Delete(userId, bookId) as OkObjectResult;

        Assert.That(result, Is.Not.Null);

        var messageProperty = result!.Value!.GetType().GetProperty("message");
        var message = messageProperty!.GetValue(result.Value)?.ToString();

        Assert.That(message, Is.EqualTo("Livro deletado com sucesso do usuário"));
    }

    [Test]
    public void AddRating_WithValidData_ReturnsCorrectMessage()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var dto = new AddRatingDTO { Rating = 4 };

        var result = _controller.AddRating(userId, bookId, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);

        var messageProperty = result!.Value!.GetType().GetProperty("message");
        var message = messageProperty!.GetValue(result.Value)?.ToString();

        Assert.That(message, Is.EqualTo("Avaliação atualizada com sucesso!"));
    }

    [Test]
    public void AddBookToUser_WithInvalidModelState_ShouldNotCallService()
    {
        var userId = Guid.NewGuid();
        var dto = new AddBooksUserDTO();

        _controller.ModelState.AddModelError("BookId", "Required");

        var result = _controller.AddBookToUser(userId, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        _mockUserBookService.Verify(
            s => s.AddBookUser(It.IsAny<Guid>(), It.IsAny<AddBooksUserDTO>()),
            Times.Never);
    }
    [Test]
    public void GenerateAnnualReportFile_WithValidData_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2026;
        var expectedReport = new AnnualReadingReportDTO
        {
            UserName = "Leitor Incrível",
            MemberSince = "01/01/2024",
            TimeOnPlatform = "1.000.000 minutos",
            TotalRead = 12,
            TotalReading = 2,
            TotalWantToRead = 5,
            TotalPagesRead = 3000,
            EstimatedReadingHours = 50.5,
            AverageRating = 4.5,
            FavoriteGenre = "Fantasia"
        };

        _mockUserBookService
            .Setup(s => s.GenerateAnnualReport(userId, year))
            .Returns(expectedReport);

        // Act
        var result = _controller.GenerateAnnualReportFile(userId, year);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value!.ToString(), Does.Contain("Relatório gerado com sucesso"));
        Assert.That(okResult.Value.ToString(), Does.Contain($"AnnualReadingReport_{userId}_{year}.txt"));

        _mockUserBookService.Verify(s => s.GenerateAnnualReport(userId, year), Times.Once);
    }
    
    [Test]
    public void GenerateAnnualReportFile_UserNotFound_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2026;

        _mockUserBookService
            .Setup(s => s.GenerateAnnualReport(userId, year))
            .Throws(new ArgumentException($"Usuário com ID {userId} não encontrado"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _controller.GenerateAnnualReportFile(userId, year));
    }

    [Test]
    public void GenerateAnnualReportFile_WhenServiceThrowsGenericException_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2026;

        _mockUserBookService
            .Setup(s => s.GenerateAnnualReport(userId, year))
            .Throws(new Exception("Erro inesperado"));

        // Act & Assert
        Assert.Throws<Exception>(() => _controller.GenerateAnnualReportFile(userId, year));
    }
}