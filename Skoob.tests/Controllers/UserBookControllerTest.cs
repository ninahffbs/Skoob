using NUnit.Framework;
using Moq;
using Skoob.Controllers;
using Skoob.DTOs;
using Skoob.Enums;
using Skoob.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Skoob.Tests.Controllers;

[TestFixture]
public class UserBookControllerTest
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
}