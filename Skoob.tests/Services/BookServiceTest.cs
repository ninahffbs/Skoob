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

namespace Skoob.Tests
{
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

            var result = _service.GetAllBooks(page);

            _bookRepositoryMock.Verify(r => r.GetAllBooks(1, 10), Times.Once);
        }

        [Test]
        public void FilterBookByGenre_ShouldReturnFilteredList_WhenGenreMatches()
        {
            var page = 1;
            var searchGenre = "Fantasy";
            var genre1 = new Genre { Name = "Fantasy" };
            var genre2 = new Genre { Name = "Horror" };
            var author = new Author { Name = "Author" };

            var allBooks = new List<Book>
            {
                new Book { Title = "Harry Potter", Genres = new List<Genre> { genre1 }, Author = author },
                new Book { Title = "It", Genres = new List<Genre> { genre2 }, Author = author }
            };

            _bookRepositoryMock.Setup(r => r.GetAllBooks(page, 10)).Returns(allBooks);

            var result = _service.FilterBookByGenre(searchGenre, page);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Harry Potter"));
        }

        [Test]
        public void FilterBookByGenre_ShouldThrowException_WhenGenreIsInvalid()
        {
            var invalidGenre = "Fi"; // Less than 3 chars

            var ex = Assert.Throws<ArgumentException>(() => _service.FilterBookByGenre(invalidGenre, 1));

            Assert.That(ex.Message, Does.Contain("pelo menos 3 caracteres"));
        }

        [Test]
        public void FilterBookByGenre_ShouldUsePageOne_WhenPageIsInvalid()
        {
            var page = -5;
            var searchGenre = "Fantasy";
            var allBooks = new List<Book>();

            _bookRepositoryMock.Setup(r => r.GetAllBooks(1, 10)).Returns(allBooks);

            _service.FilterBookByGenre(searchGenre, page);

            _bookRepositoryMock.Verify(r => r.GetAllBooks(1, 10), Times.Once);
        }

        [Test]
        public void FilterBookByTitle_ShouldReturnFilteredList_WhenTitleMatches()
        {
            var page = 1;
            var searchTitle = "Potter";
            var genre = new Genre { Name = "Fantasy" };
            var author = new Author { Name = "Author" };

            var allBooks = new List<Book>
            {
                new Book { Title = "Harry Potter", Genres = new List<Genre> { genre }, Author = author },
                new Book { Title = "Lord of the Rings", Genres = new List<Genre> { genre }, Author = author }
            };

            _bookRepositoryMock.Setup(r => r.GetAllBooks(page, 10)).Returns(allBooks);

            var result = _service.FilterBookByTitle(searchTitle, page);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Harry Potter"));
        }

        [Test]
        public void FilterBookByTitle_ShouldThrowException_WhenTitleIsInvalid()
        {
            var invalidTitle = "HP"; // Less than 3 chars

            var ex = Assert.Throws<ArgumentException>(() => _service.FilterBookByTitle(invalidTitle, 1));

            Assert.That(ex.Message, Does.Contain("pelo menos 3 caracteres"));
        }
        
        [Test]
        public void FilterBookByTitle_WhenPageIsZero_ShouldSetPageToOne()
        {
            var searchedTitle = "har";

            var books = new List<Book>
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Harry Potter",
                    PagesNumber = 300,
                    Synopsis = "Test",
                    PublishingYear = 2000,
                    Author = new Author { Name = "J.K Rowling" },
                    Genres = new List<Genre> { new Genre { Name = "Fantasy" } }
                }
            };

            _bookRepositoryMock
                .Setup(x => x.GetAllBooks(1, It.IsAny<int>()))
                .Returns(books);

            var result = _service.FilterBookByTitle(searchedTitle, 0);

            Assert.That(result.Count, Is.EqualTo(1));

            _bookRepositoryMock.Verify(x => x.GetAllBooks(1, It.IsAny<int>()), Times.Once);
        }
    }
}