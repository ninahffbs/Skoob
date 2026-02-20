using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Configuration;
using Skoob.Services;
using Skoob.Interfaces;
using Skoob.DTOs;
using Skoob.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skoob.Tests;

[TestFixture]
public class BookServiceTests
{
    private Mock<IBookRepository> _bookRepositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private BookService _service;

    [SetUp]
    public void Setup()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _configurationMock = new Mock<IConfiguration>();

        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(x => x.Value).Returns("10");
        _configurationMock.Setup(x => x.GetSection("Pagination:UsersPageSize")).Returns(configurationSectionMock.Object);

        _service = new BookService(
            _bookRepositoryMock.Object,
            _configurationMock.Object
        );
    }

    // --- Testes GetAllBooks ---

    [Test]
    public void GetAllBooks_ShouldReturnMappedDTOs_WhenBooksExist()
    {
        var page = 1;
        var genre = new Genre { Name = "Fantasy" };
        var author = new Author { Name = "J.K. Rowling" };
        var books = new List<Book>
        {
            new Book { Id = Guid.NewGuid(), Title = "Book 1", PagesNumber = 300, Genres = new List<Genre> { genre }, Author = author },
            new Book { Id = Guid.NewGuid(), Title = "Book 2", PagesNumber = 150, Genres = new List<Genre> { genre }, Author = author }
        };

        _bookRepositoryMock.Setup(r => r.GetAllBooks(page, 10)).Returns(books);

        var result = _service.GetAllBooks(page);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("Book 1"));
        Assert.That(result[0].AuthorName, Is.EqualTo("J.K. Rowling"));
        _bookRepositoryMock.Verify(r => r.GetAllBooks(page, 10), Times.Once);
    }

    [Test]
    public void GetAllBooks_ShouldUsePageOne_WhenPageIsZeroOrNegative()
    {
        var page = 0;
        var books = new List<Book>();
        _bookRepositoryMock.Setup(r => r.GetAllBooks(1, 10)).Returns(books);

        _service.GetAllBooks(page);

        _bookRepositoryMock.Verify(r => r.GetAllBooks(1, 10), Times.Once);
    }

    // --- Testes FilterBookByGenre ---

    [Test]
    public void FilterBookByGenre_ShouldReturnFilteredList_WhenGenreMatches()
    {
        var page = 1;
        var searchGenre = "Fantasy";
        var author = new Author { Name = "Author" };
        var books = new List<Book>
        {
            new Book { Title = "Harry Potter", Genres = new List<Genre> { new Genre { Name = "Fantasy" } }, Author = author }
        };

        _bookRepositoryMock.Setup(r => r.GetBooksByGenre(searchGenre, page, 10)).Returns(books);

        var result = _service.FilterBookByGenre(searchGenre, page);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Harry Potter"));
        _bookRepositoryMock.Verify(r => r.GetBooksByGenre(searchGenre, page, 10), Times.Once);
    }

    [Test]
    public void FilterBookByGenre_ShouldThrowException_WhenGenreIsInvalid()
    {
        var invalidGenre = "Fi"; 
        var ex = Assert.Throws<ArgumentException>(() => _service.FilterBookByGenre(invalidGenre, 1));
        Assert.That(ex.Message, Does.Contain("pelo menos 3 caracteres"));
    }

    [Test]
    public void FilterBookByGenre_ShouldUsePageOne_WhenPageIsInvalid()
    {
        var page = -5;
        var searchGenre = "Fantasy";
        _bookRepositoryMock.Setup(r => r.GetBooksByGenre(searchGenre, 1, 10)).Returns(new List<Book>());

        _service.FilterBookByGenre(searchGenre, page);

        _bookRepositoryMock.Verify(r => r.GetBooksByGenre(searchGenre, 1, 10), Times.Once);
    }

    // --- Testes FilterBookByTitle ---

    [Test]
    public void FilterBookByTitle_ShouldReturnFilteredList_WhenTitleMatches()
    {
        var page = 1;
        var searchTitle = "Potter";
        var books = new List<Book>
        {
            new Book { Title = "Harry Potter", Genres = new List<Genre>(), Author = new Author { Name = "A" } }
        };

        _bookRepositoryMock.Setup(r => r.GetBooksByTitle(searchTitle, page, 10)).Returns(books);

        var result = _service.FilterBookByTitle(searchTitle, page);

        Assert.That(result.Count, Is.EqualTo(1));
        _bookRepositoryMock.Verify(r => r.GetBooksByTitle(searchTitle, page, 10), Times.Once);
    }

    [Test]
    public void FilterBookByTitle_ShouldThrowException_WhenTitleIsInvalid()
    {
        var invalidTitle = "HP"; 
        var ex = Assert.Throws<ArgumentException>(() => _service.FilterBookByTitle(invalidTitle, 1));
        Assert.That(ex.Message, Does.Contain("pelo menos 3 caracteres"));
    }

    [Test]
    public void FilterBookByTitle_WhenPageIsZero_ShouldSetPageToOne()
    {
        var searchedTitle = "Harry";
        _bookRepositoryMock.Setup(r => r.GetBooksByTitle(searchedTitle, 1, 10)).Returns(new List<Book>());

        _service.FilterBookByTitle(searchedTitle, 0);

        _bookRepositoryMock.Verify(r => r.GetBooksByTitle(searchedTitle, 1, 10), Times.Once);
    }

    // --- Testes FilterBookByAuthor ---

    [Test]
    public void FilterBookByAuthor_ShouldReturnFilteredList_WhenAuthorMatches()
    {
        var page = 1;
        var searchAuthor = "Rowling";
        var books = new List<Book>
        {
            new Book { Title = "HP", Author = new Author { Name = "J.K. Rowling" }, Genres = new List<Genre>() }
        };

        _bookRepositoryMock.Setup(r => r.GetBooksByAuthor(searchAuthor, page, 10)).Returns(books);

        var result = _service.FilterBookByAuthor(searchAuthor, page);

        Assert.That(result.Count, Is.EqualTo(1));
        _bookRepositoryMock.Verify(r => r.GetBooksByAuthor(searchAuthor, page, 10), Times.Once);
    }

    [Test]
    public void FilterBookByAuthor_ShouldThrowException_WhenAuthorNameIsInvalid()
    {
        var invalidAuthor = "Al"; 
        var ex = Assert.Throws<ArgumentException>(() => _service.FilterBookByAuthor(invalidAuthor, 1));
        Assert.That(ex.Message, Does.Contain("pelo menos 3 caracteres"));
    }

    [Test]
    public void FilterBookByAuthor_ShouldUsePageOne_WhenPageIsZeroOrNegative()
    {
        var invalidPage = -5;
        var searchAuthor = "Tolkien";
        _bookRepositoryMock.Setup(r => r.GetBooksByAuthor(searchAuthor, 1, 10)).Returns(new List<Book>());

        _service.FilterBookByAuthor(searchAuthor, invalidPage);

        _bookRepositoryMock.Verify(r => r.GetBooksByAuthor(searchAuthor, 1, 10), Times.Once);
    }

    [Test]
    public void FilterBookByAuthor_ShouldReturnEmptyList_WhenNoAuthorMatches()
    {
        var page = 1;
        var searchAuthor = "Unknown";
        _bookRepositoryMock.Setup(r => r.GetBooksByAuthor(searchAuthor, page, 10)).Returns(new List<Book>());

        var result = _service.FilterBookByAuthor(searchAuthor, page);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        _bookRepositoryMock.Verify(r => r.GetBooksByAuthor(searchAuthor, page, 10), Times.Once);
    }
}