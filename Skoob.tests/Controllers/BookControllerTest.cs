using Moq;
using Microsoft.AspNetCore.Mvc;
using Skoob.Controllers;
using Skoob.Interfaces;
using Skoob.DTOs;

namespace Skoob.Tests;

[TestFixture]
public class BookControllerTests
{
    private Mock<IBookService> _bookServiceMock;
    private BookController _controller;

    [SetUp]
    public void Setup()
    {
        _bookServiceMock = new Mock<IBookService>();
        _controller = new BookController(_bookServiceMock.Object);
    }

    [Test]
    public void ListAllBooks_ShouldReturnOk_WhenBooksExist()
    {
        var page = 1;
        var mockBooks = new List<BookDTO> { new BookDTO(), new BookDTO() };
        _bookServiceMock.Setup(s => s.GetAllBooks(page)).Returns(mockBooks);

        var result = _controller.ListAllBooks(page);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(mockBooks));
        _bookServiceMock.Verify(s => s.GetAllBooks(page), Times.Once);
    }

    [Test]
    public void FilterBookByTitle_ShouldReturnOk_WhenServiceReturnsList()
    {
        var title = "Harry";
        var page = 1;
        var mockBooks = new List<BookDTO> { new BookDTO() };
        _bookServiceMock.Setup(s => s.FilterBookByTitle(title, page)).Returns(mockBooks);

        var result = _controller.FilterBookByTitle(title, page);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(mockBooks));
        _bookServiceMock.Verify(s => s.FilterBookByTitle(title, page), Times.Once);
    }

    [Test]
    public void FilterBookByGenre_ShouldReturnOk_WhenServiceReturnsList()
    {
        var genre = "Fantasy";
        var page = 1;
        var mockBooks = new List<BookDTO> { new BookDTO() };
        _bookServiceMock.Setup(s => s.FilterBookByGenre(genre, page)).Returns(mockBooks);

        var result = _controller.FilterBookByGenre(genre, page);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(mockBooks));
    }

    [Test]
    public void FilterBookByGenre_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
    {
        var genre = "Fa"; 
        var page = 1;
        var expectedMessage = "O gênero buscado deve ter pelo menos 3 caracteres.";
        
        _bookServiceMock.Setup(s => s.FilterBookByGenre(genre, page))
            .Throws(new ArgumentException(expectedMessage));

        var result = _controller.FilterBookByGenre(genre, page);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        
        var value = badRequestResult.Value;
        Assert.That(value?.ToString(), Does.Contain(expectedMessage));
    }

    [Test]
    public void FilterBookByAuthor_ShouldReturnOk_WhenServiceReturnsData()
    {
        var author = "Tolkien";
        var page = 1;
        var mockBooks = new List<BookDTO> { new BookDTO { Title = "O Hobbit", AuthorName = "J.R.R. Tolkien" } };

        _bookServiceMock.Setup(s => s.FilterBookByAuthor(author, page)).Returns(mockBooks);

        var result = _controller.FilterBookByAuthor(author, page);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(mockBooks));
        _bookServiceMock.Verify(s => s.FilterBookByAuthor(author, page), Times.Once);
    }

    [Test]
    public void FilterBookByAuthor_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
    {
        var invalidAuthor = "Al";
        var page = 1;
        var expectedError = "O nome do autor deve ter pelo menos 3 caracteres.";

        _bookServiceMock.Setup(s => s.FilterBookByAuthor(invalidAuthor, page))
                        .Throws(new ArgumentException(expectedError));

        var result = _controller.FilterBookByAuthor(invalidAuthor, page);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        Assert.That(badRequestResult.Value?.ToString(), Does.Contain(expectedError));
        _bookServiceMock.Verify(s => s.FilterBookByAuthor(invalidAuthor, page), Times.Once);
    }
}
