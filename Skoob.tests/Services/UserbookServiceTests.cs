using Moq;
using Microsoft.Extensions.Configuration;
using Skoob.Services;
using Skoob.Interfaces;
using Skoob.DTOs;
using Skoob.Models;
using Skoob.Enums;


namespace Skoob.Tests
{
    [TestFixture]
    public class UserbookServiceTests
    {
        private Mock<IUserbookRepository> _userbookRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IBookRepository> _bookRepositoryMock;
        private Mock<IConfiguration> _configurationMock;
        private UserbookService _service;

        [SetUp]
        public void Setup()
        {
            _userbookRepositoryMock = new Mock<IUserbookRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _bookRepositoryMock = new Mock<IBookRepository>();
            _configurationMock = new Mock<IConfiguration>();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(x => x.Value).Returns("10");
            _configurationMock.Setup(x => x.GetSection("Pagination:UsersPageSize")).Returns(configurationSectionMock.Object);

            _service = new UserbookService(
                _userbookRepositoryMock.Object,
                _userRepositoryMock.Object,
                _bookRepositoryMock.Object,
                _configurationMock.Object
            );
        }

        [Test]
        public void AddBookUser_ShouldAddBook_WhenValid()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var dto = new AddBooksUserDTO { BookId = bookId, Status = StatusBook.Lendo, PagesRead = 10 };
            var book = new Book { Id = bookId, PagesNumber = 100, Title = "Test Book", Author = new Author { Name = "Author" } };
            var userbook = new Userbook { UserId = userId, BookId = bookId, Book = book, Status = StatusBook.Lendo, PagesRead = 10 };

            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.BookExists(bookId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.UserHasBook(userId, bookId)).Returns(false);
            _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);
            _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userbook);

            var result = _service.AddBookUser(userId, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.BookId, Is.EqualTo(bookId));
            _userbookRepositoryMock.Verify(x => x.AddBookToUser(It.IsAny<Userbook>()), Times.Once);
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenStatusInvalid()
        {
            var userId = Guid.NewGuid();
            var dto = new AddBooksUserDTO { Status = (StatusBook)99 };

            Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var dto = new AddBooksUserDTO { Status = StatusBook.Lendo };
            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(false);

            Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenPagesReadExceedsTotal()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var dto = new AddBooksUserDTO { BookId = bookId, Status = StatusBook.Lendo, PagesRead = 200 };
            var book = new Book { Id = bookId, PagesNumber = 100, Title = "Test Book" };

            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.BookExists(bookId)).Returns(true);
            _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

            Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
        }

        [Test]
        public void GetUserBooks_ShouldReturnList_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var book = new Book { Title = "Title", PagesNumber = 100, Author = new Author { Name = "Author" } };
            var list = new List<Userbook>
            {
                new Userbook { UserId = userId, Book = book, PagesRead = 10, Status = StatusBook.Lendo }
            };

            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.GetUserbooksByUserId(userId)).Returns(list);

            var result = _service.GetUserBooks(userId);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].BookTitle, Is.EqualTo("Title"));
        }

        [Test]
        public void RemoveUserBook_ShouldThrowException_WhenDeleteFails()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            _userbookRepositoryMock.Setup(x => x.DeleteUserBook(userId, bookId)).Returns(false);

            Assert.Throws<ArgumentException>(() => _service.RemoveUserBook(userId, bookId));
        }

        [Test]
        public void UpdateReadPages_ShouldUpdateStatusToLido_WhenPagesFinished()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, PagesNumber = 100 };
            var userbook = new Userbook { UserId = userId, BookId = bookId, Book = book, Status = StatusBook.Lendo };

            _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userbook);
            _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

            _service.UpdateReadPages(userId, bookId, 100);

            Assert.That(userbook.Status, Is.EqualTo(StatusBook.Lido));
            Assert.That(userbook.FinishDate, Is.Not.Null);
            _userbookRepositoryMock.Verify(x => x.UpdateReadPages(userbook), Times.Once);
        }

        [Test]
        public void AddRating_ShouldThrowException_WhenBookNotFinished()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var userbook = new Userbook { Status = StatusBook.Lendo };

            _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userbook);

            Assert.Throws<ArgumentException>(() => _service.AddRating(userId, bookId, 5));
        }

        [Test]
        public void FilterUserBookByTitle_ShouldReturnFilteredList()
        {
            var userId = Guid.NewGuid();
            var book1 = new Book { Title = "Harry Potter" };
            var book2 = new Book { Title = "Lord of the Rings" };
            var list = new List<Userbook>
            {
                new Userbook { Book = book1 },
                new Userbook { Book = book2 }
            };

            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(new Mainuser());
            _userbookRepositoryMock.Setup(x => x.GetUserbooksByUserId(userId)).Returns(list);

            var result = _service.FilterUserBookByTitle(userId, "Harry");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].BookTitle, Is.EqualTo("Harry Potter"));
        }

        [Test]
        public void FilterUserBookByGenre_ShouldReturnFilteredList()
        {
            var userId = Guid.NewGuid();
            var genre1 = new Genre { Name = "Fantasy" };
            var genre2 = new Genre { Name = "Sci-Fi" };
            var book1 = new Book { Title = "Book1", Genres = new List<Genre> { genre1 } };
            var book2 = new Book { Title = "Book2", Genres = new List<Genre> { genre2 } };

            var list = new List<Userbook>
            {
                new Userbook { Book = book1 },
                new Userbook { Book = book2 }
            };

            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(new Mainuser());
            _userbookRepositoryMock.Setup(x => x.GetUserbooksByUserId(userId)).Returns(list);

            var result = _service.FilterUserBookByGenre(userId, "Fantasy");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].BookTitle, Is.EqualTo("Book1"));
        }
        [Test]
        public void AddBookUser_ShouldThrowException_WhenUserAlreadyHasBook()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var dto = new AddBooksUserDTO
            {
                BookId = bookId,
                Status = StatusBook.Lendo
            };

            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.BookExists(bookId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.UserHasBook(userId, bookId)).Returns(true);

            Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenBookNotFound()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var dto = new AddBooksUserDTO
            {
                BookId = bookId,
                Status = StatusBook.Lendo
            };

            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.BookExists(bookId)).Returns(false);

            Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
        }
        [Test]
        public void AddBookUser_WhenPagesReadEqualsTotal_ShouldSetStatusToLido()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var dto = new AddBooksUserDTO
            {
                BookId = bookId,
                Status = StatusBook.Lendo,
                PagesRead = 100
            };

            var book = new Book
            {
                Id = bookId,
                PagesNumber = 100,
                Title = "Test"
            };

            _userRepositoryMock.Setup(x => x.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.BookExists(bookId)).Returns(true);
            _userbookRepositoryMock.Setup(x => x.UserHasBook(userId, bookId)).Returns(false);
            _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

            _userbookRepositoryMock
                .Setup(x => x.GetUserbook(userId, bookId))
                .Returns(new Userbook
                {
                    BookId = bookId,
                    Book = book,
                    Status = StatusBook.Lido
                });

            var result = _service.AddBookUser(userId, dto);

            Assert.That(result.Status, Is.EqualTo(StatusBook.Lido));
        }

        [Test]
        public void UpdateReadPages_WhenPagesGreaterThanZero_ShouldSetStatusToLendo()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var book = new Book { Id = bookId, PagesNumber = 200 };

            var userbook = new Userbook
            {
                UserId = userId,
                BookId = bookId,
                Book = book,
                Status = StatusBook.QueroLer
            };

            _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userbook);
            _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

            _service.UpdateReadPages(userId, bookId, 50);

            Assert.That(userbook.Status, Is.EqualTo(StatusBook.Lendo));
        }

        [Test]
        public void UpdateReadPages_WhenPagesNegative_ShouldThrowException()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _service.UpdateReadPages(userId, bookId, -5));
        }

        [Test]
        public void AddRating_WhenRatingNegative_ShouldThrowException()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _service.AddRating(userId, bookId, -1));
        }

        [Test]
        public void AddRating_WhenBookFinished_ShouldAddRating()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var userbook = new Userbook
            {
                Status = StatusBook.Lido
            };

            _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userbook);

            _service.AddRating(userId, bookId, 5);

            Assert.That(userbook.Rating, Is.EqualTo(5));
            _userbookRepositoryMock.Verify(x => x.AddRating(userbook), Times.Once);
        }

        [Test]
        public void FilterUserBookByTitle_WhenTitleTooShort_ShouldThrowException()
        {
            var userId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _service.FilterUserBookByTitle(userId, "ab"));
        }

        [Test]
        public void FilterUserBookByGenre_WhenGenreTooShort_ShouldThrowException()
        {
            var userId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _service.FilterUserBookByGenre(userId, "a"));
        }
        [Test]
        public void AddBookUser_WhenStatusIsQueroLer_ShouldSetStartDateToNull()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var dto = new AddBooksUserDTO
            {
                BookId = bookId,
                Status = StatusBook.QueroLer,
                StartDate = DateTime.UtcNow.AddDays(-3),
                PagesRead = 0
            };

            var book = new Book
            {
                Id = bookId,
                Title = "Livro Teste",
                PagesNumber = 100
            };

            var createdUserBook = new Userbook
            {
                UserId = userId,
                BookId = bookId,
                Status = StatusBook.QueroLer,
                PagesRead = 0,
                StartDate = null,
                Book = book
            };

            _userRepositoryMock
                .Setup(x => x.Exists(userId))
                .Returns(true);

            _userbookRepositoryMock
                .Setup(x => x.BookExists(bookId))
                .Returns(true);

            _userbookRepositoryMock
                .Setup(x => x.UserHasBook(userId, bookId))
                .Returns(false);

            _bookRepositoryMock
                .Setup(x => x.GetBookById(bookId))
                .Returns(book);

            _userbookRepositoryMock
                .Setup(x => x.AddBookToUser(It.IsAny<Userbook>()));

            _userbookRepositoryMock
                .Setup(x => x.GetUserbook(userId, bookId))
                .Returns(createdUserBook);

            var result = _service.AddBookUser(userId, dto);

            Assert.That(result.StartDate, Is.Null);
        }


        [Test]
        public void UpdateReadPages_WhenPagesGreaterThanTotal_ShouldThrowException()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var book = new Book
            {
                Id = bookId,
                PagesNumber = 200
            };

            var userBook = new Userbook
            {
                UserId = userId,
                BookId = bookId,
                Book = book
            };

            _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId))
                .Returns(userBook);

            _bookRepositoryMock.Setup(x => x.GetBookById(bookId))
                .Returns(book);

            var ex = Assert.Throws<ArgumentException>(() =>
                _service.UpdateReadPages(userId, bookId, 250));

            Assert.That(ex.Message,
                Is.EqualTo($"Este livro possui apenas {book.PagesNumber} páginas"));
        }
        [Test]
        public void FilterUserBookByTitle_WhenUserNotFound_ShouldThrowException()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(x => x.GetById(userId))
                .Returns((Mainuser?)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _service.FilterUserBookByTitle(userId, "harry"));

            Assert.That(ex.Message,
                Is.EqualTo("Usuário com esse id não foi encontrado."));
        }

        [Test]
        public void FilterUserBookByGenre_WhenUserNotFound_ShouldThrowException()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(x => x.GetById(userId))
                .Returns((Mainuser?)null);

            var ex = Assert.Throws<ArgumentException>(() =>
                _service.FilterUserBookByGenre(userId, "fantasy"));

            Assert.That(ex.Message,
                Is.EqualTo("Usuário com esse id não foi encontrado."));
        }
        [Test]
        public void GenerateAnnualReport_UserNotFound_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns((Mainuser?)null);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.GenerateAnnualReport(userId, 2026));
            Assert.That(ex.Message, Does.Contain("não encontrado"));
        }

        [Test]
        public void GenerateAnnualReport_WithValidData_ShouldCalculateCorrectStatistics()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var user = new Mainuser
            {
                Id = userId,
                UserName = "DevUser",
                CreatedAt = DateTime.UtcNow.AddDays(-1) // 1 dia atrás = 1440 minutos
            };

            var genreFantasy = new Genre { Name = "Fantasia" };
            var genreSciFi = new Genre { Name = "Ficção" };

            var books = new List<Userbook>
            {
                // Livro Lido este ano (deve contar em tudo)
                new Userbook {
                    Status = StatusBook.Lido,
                    FinishDate = new DateTime(year, 5, 10),
                    Rating = 5,
                    Book = new Book { PagesNumber = 100, Genres = new List<Genre> { genreFantasy } }
                },
                // Livro sendo lido (deve contar apenas páginas lidas e horas)
                new Userbook {
                    Status = StatusBook.Lendo,
                    PagesRead = 50,
                    Book = new Book { PagesNumber = 200, Genres = new List<Genre> { genreSciFi } }
                },
                // Livro 'Quero Ler' (deve contar apenas no total da lista)
                new Userbook {
                    Status = StatusBook.QueroLer,
                    Book = new Book { PagesNumber = 300 }
                }
            };

            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(user);
            _userbookRepositoryMock.Setup(x => x.GetUserbooksByUserId(userId)).Returns(books);

            // Act
            var report = _service.GenerateAnnualReport(userId, year);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(report.UserName, Is.EqualTo("DevUser"));
                Assert.That(report.TotalRead, Is.EqualTo(1));
                Assert.That(report.TotalReading, Is.EqualTo(1));
                Assert.That(report.TotalWantToRead, Is.EqualTo(1));

                // Páginas: 100 (lido) + 50 (lendo) = 150
                Assert.That(report.TotalPagesRead, Is.EqualTo(150));

                // Horas: 150 / 60 = 2.5
                Assert.That(report.EstimatedReadingHours, Is.EqualTo(2.5));

                Assert.That(report.AverageRating, Is.EqualTo(5.0));
                Assert.That(report.FavoriteGenre, Is.EqualTo("Fantasia"));

                // Tempo na plataforma (deve conter "1.440 minutos" aproximadamente)
                Assert.That(report.TimeOnPlatform, Does.Contain("minutos"));
                Assert.That(report.MemberSince, Is.EqualTo(user.CreatedAt.Value.ToString("dd/MM/yyyy")));
            });
        }

        [Test]
        public void GenerateAnnualReport_NoReadBooks_ShouldReturnZeroAverageRating()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new Mainuser { Id = userId, CreatedAt = DateTime.UtcNow };

            // Lista vazia de livros
            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(user);
            _userbookRepositoryMock.Setup(x => x.GetUserbooksByUserId(userId)).Returns(new List<Userbook>());

            // Act
            var report = _service.GenerateAnnualReport(userId, 2026);

            // Assert
            Assert.That(report.AverageRating, Is.EqualTo(0));
            Assert.That(report.FavoriteGenre, Is.Null);
            Assert.That(report.TotalPagesRead, Is.EqualTo(0));
        }
    }
}