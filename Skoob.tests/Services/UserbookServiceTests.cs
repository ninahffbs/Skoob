using Moq;
using Microsoft.Extensions.Configuration;
using Skoob.Services;
using Skoob.Interfaces;
using Skoob.DTOs;
using Skoob.Models;
using Skoob.Enums;


namespace Skoob.Tests;

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
        _userbookRepositoryMock.Verify(x => x.Update(userbook), Times.Once);
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
        _userbookRepositoryMock.Verify(x => x.Update(userbook), Times.Once);
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
    public void FilterUserBookByAuthor_ShouldThrowException_WhenAuthorIsInvalid()
    {
        var userId = Guid.NewGuid();
        var invalidAuthor = "Jo";

        var ex = Assert.Throws<ArgumentException>(() => _service.FilterUserBookByAuthor(userId, invalidAuthor));
        Assert.That(ex.Message, Does.Contain("pelo menos 3 caracteres"));
    }

    [Test]
    public void FilterUserBookByAuthor_ShouldThrowException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        var validAuthor = "Tolkien";
        _userRepositoryMock.Setup(x => x.GetById(userId)).Returns((Mainuser?)null);

        var ex = Assert.Throws<ArgumentException>(() => _service.FilterUserBookByAuthor(userId, validAuthor));
        Assert.That(ex.Message, Does.Contain("Usuário com este id não foi encontrado"));
    }

    [Test]
    public void FilterUserBookByAuthor_ShouldReturnFilteredList_WhenAuthorMatchesCaseInsensitive()
    {
        var userId = Guid.NewGuid();
        var searchAuthor = "tolkien";

        var author1 = new Author { Name = "J.R.R. Tolkien" };
        var author2 = new Author { Name = "George R.R. Martin" };

        var book1 = new Book { Title = "Senhor dos Anéis", Author = author1, PagesNumber = 1000 };
        var book2 = new Book { Title = "Game of Thrones", Author = author2, PagesNumber = 800 };

        var list = new List<Userbook>
        {
            new() { UserId = userId, BookId = Guid.NewGuid(), Book = book1, Status = StatusBook.Lendo },
            new() { UserId = userId, BookId = Guid.NewGuid(), Book = book2, Status = StatusBook.Lido }
        };

        _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(new Mainuser { Id = userId });
        _userbookRepositoryMock.Setup(x => x.GetUserbooksByUserId(userId)).Returns(list);

        var result = _service.FilterUserBookByAuthor(userId, searchAuthor);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1), "Deveria ter filtrado apenas 1 livro.");
        Assert.That(result[0].AuthorName, Is.EqualTo("J.R.R. Tolkien"));
    }

    [Test]
    public void UpdateStatus_ShouldThrowException_WhenUserBookNotFound()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns((Userbook?)null);

        var ex = Assert.Throws<ArgumentException>(() => _service.UpdateStatus(userId, bookId, StatusBook.Lido));
        Assert.That(ex.Message, Does.Contain("Livro não encontrado na biblioteca deste usuário"));
    }

    [Test]
    public void UpdateStatus_ShouldThrowException_WhenStatusIsInvalid()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userBook = new Userbook { UserId = userId, BookId = bookId };

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);

        var invalidStatus = (StatusBook)999; 

        var ex = Assert.Throws<ArgumentException>(() => _service.UpdateStatus(userId, bookId, invalidStatus));
        Assert.That(ex.Message, Does.Contain("não é um status válido para um livro"));
    }

    [Test]
    public void UpdateStatus_ShouldUpdateToLido_AndSetFinishDate_AndMaxPages()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, PagesNumber = 500 };
        var userBook = new Userbook 
        { 
            UserId = userId, 
            BookId = bookId, 
            PagesRead = 100,
            StartDate = null, 
            FinishDate = null 
        };

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);
        _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

        _service.UpdateStatus(userId, bookId, StatusBook.Lido);

        Assert.That(userBook.Status, Is.EqualTo(StatusBook.Lido));
        Assert.That(userBook.FinishDate, Is.Not.Null);
        Assert.That(userBook.StartDate, Is.Not.Null, "Deveria ter setado a data de início também, já que estava nula.");
        Assert.That(userBook.PagesRead, Is.EqualTo(500), "Deveria ter atualizado as páginas lidas para o total do livro.");
        
        _userbookRepositoryMock.Verify(x => x.Update(userBook), Times.Once);
    }

    public void UpdateStatus_ShouldUpdateToLendo_AndClearFinishDate()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, PagesNumber = 500 };
        var userBook = new Userbook 
        { 
            UserId = userId, 
            BookId = bookId, 
            Status = StatusBook.Lido, // Simula que estava Lido
            FinishDate = DateTime.UtcNow.AddDays(-1), // Tinha data de fim
            StartDate = DateTime.UtcNow.AddDays(-5)
        };

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);
        _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

        _service.UpdateStatus(userId, bookId, StatusBook.Lendo);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userBook.Status, Is.EqualTo(StatusBook.Lendo));
            Assert.That(userBook.FinishDate, Is.Null, "Deveria ter limpado a data de término ao voltar para 'Lendo'.");
            Assert.That(userBook.StartDate, Is.Not.Null, "Não deveria ter apagado a data de início que já existia.");
        }

        _userbookRepositoryMock.Verify(x => x.Update(userBook), Times.Once);
    }

    [Test]
    public void UpdateStatus_ShouldUpdateToQueroLer_AndClearAllProgress()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, PagesNumber = 500 };
        var userBook = new Userbook 
        { 
            UserId = userId, 
            BookId = bookId, 
            Status = StatusBook.Lendo,
            PagesRead = 250,
            StartDate = DateTime.UtcNow,
            FinishDate = DateTime.UtcNow 
        };

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);
        _bookRepositoryMock.Setup(x => x.GetBookById(bookId)).Returns(book);

        _service.UpdateStatus(userId, bookId, StatusBook.QueroLer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userBook.Status, Is.EqualTo(StatusBook.QueroLer));
            Assert.That(userBook.StartDate, Is.Null, "Status 'Quero Ler' não pode ter data de início.");
            Assert.That(userBook.FinishDate, Is.Null, "Status 'Quero Ler' não pode ter data de fim.");
            Assert.That(userBook.PagesRead, Is.EqualTo(0), "Deveria ter zerado as páginas lidas.");
        }

        _userbookRepositoryMock.Verify(x => x.Update(userBook), Times.Once);
    }

    [Test]
    public void UpdateReview_ShouldThrowArgumentException_WhenUserBookNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns((Userbook)null);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.UpdateReview(userId, bookId, "Ótimo livro"));
        
        Assert.That(ex.Message, Does.Contain("Livro não encontrado"));
        _userbookRepositoryMock.Verify(x => x.Update(It.IsAny<Userbook>()), Times.Never);
    }

    [Test]
    public void UpdateReview_ShouldThrowInvalidOperationException_WhenStatusIsQueroLer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userBook = new Userbook { UserId = userId, BookId = bookId, Status = StatusBook.Lendo };

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _service.UpdateReview(userId, bookId, "Tentando avaliar antes de ler"));
        
        Assert.That(ex.Message, Does.Contain("não pode avaliar um livro que ainda não começou a ler"));
        _userbookRepositoryMock.Verify(x => x.Update(It.IsAny<Userbook>()), Times.Never);
    }

    [Test]
    public void UpdateReview_ShouldUpdateSuccessfully_AndTrimText_WhenStatusIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userBook = new Userbook { UserId = userId, BookId = bookId, Status = StatusBook.Lido }; 
        var untrimmedReview = "   Obra prima da ficção!   "; 

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);

        // Act
        _service.UpdateReview(userId, bookId, untrimmedReview);

        // Assert
        Assert.That(userBook.Review, Is.EqualTo("Obra prima da ficção!"), "O serviço deveria ter aplicado Trim() para limpar a sujeira da string.");
        _userbookRepositoryMock.Verify(x => x.Update(userBook), Times.Once);
    }

    [Test]
    public void UpdateReview_ShouldAllowNullReview_ToClearExistingReview()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userBook = new Userbook { UserId = userId, BookId = bookId, Status = StatusBook.Lido, Review = "Achei o final ruim." };

        _userbookRepositoryMock.Setup(x => x.GetUserbook(userId, bookId)).Returns(userBook);

        // Act
        _service.UpdateReview(userId, bookId, null);

        // Assert
        Assert.That(userBook.Review, Is.Null, "O sistema não permitiu limpar a resenha existente.");
        _userbookRepositoryMock.Verify(x => x.Update(userBook), Times.Once);
    }
}

    