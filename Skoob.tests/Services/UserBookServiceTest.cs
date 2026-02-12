using NUnit.Framework;
using Moq;
using System;
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
        private UserbookService _service;

        [SetUp]
        public void Setup()
        {
            _userbookRepositoryMock = new Mock<IUserbookRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _service = new UserbookService(_userbookRepositoryMock.Object, _userRepositoryMock.Object);
        }

        // ==========================================
        // TESTS FOR: AddBookUser
        // ==========================================

        [Test]
        public void AddBookUser_ShouldChangeStatusToRead_WhenPagesReadEqualsTotal()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            
            var dto = new AddBooksUserDTO 
            { 
                BookId = bookId, 
                Status = StatusBook.Lendo, 
                PagesRead = 300 
            };

            var mockBook = new Book { Id = bookId, Title = "Test Book", PagesNumber = 300 };

            _userRepositoryMock.Setup(u => u.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.BookExists(bookId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.UserHasBook(userId, bookId)).Returns(false);
            _userbookRepositoryMock.Setup(r => r.GetBookById(bookId)).Returns(mockBook);

            _userbookRepositoryMock
                .Setup(r => r.GetUserbook(userId, bookId))
                .Returns(new Userbook { Status = StatusBook.Lido, Book = mockBook });

            _service.AddBookUser(userId, dto);

            _userbookRepositoryMock.Verify(r => r.AddBookToUser(It.Is<Userbook>(ub => 
                ub.Status == StatusBook.Lido && 
                ub.FinishDate != null
            )), Times.Once);
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenPagesReadExceedsTotal()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var dto = new AddBooksUserDTO { BookId = bookId, PagesRead = 500, Status = StatusBook.Lendo };

            var mockBook = new Book { Id = bookId, PagesNumber = 200, Title = "Clean Code" };

            _userRepositoryMock.Setup(u => u.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.BookExists(bookId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.UserHasBook(userId, bookId)).Returns(false);
            _userbookRepositoryMock.Setup(r => r.GetBookById(bookId)).Returns(mockBook);

            var ex = Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
            
            Assert.That(ex.Message, Does.Contain("200"));
            Assert.That(ex.Message, Does.Contain("Clean Code"));
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenStatusIsInvalid()
        {
            var userId = Guid.NewGuid();
            var dto = new AddBooksUserDTO 
            { 
                Status = (StatusBook)99, 
                BookId = Guid.NewGuid() 
            };

            var ex = Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
            Assert.That(ex.Message, Does.Contain("não é um status válido"));
        }

        [Test]
        public void AddBookUser_ShouldSetStartDateToNull_WhenStatusIsWantToRead()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var dto = new AddBooksUserDTO 
            { 
                BookId = bookId, 
                Status = StatusBook.QueroLer, 
                StartDate = DateTime.Now 
            };

            var mockBook = new Book { Id = bookId, PagesNumber = 100, Title = "Livro Teste" };

            _userRepositoryMock.Setup(u => u.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.BookExists(bookId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.UserHasBook(userId, bookId)).Returns(false);
            _userbookRepositoryMock.Setup(r => r.GetBookById(bookId)).Returns(mockBook);
        
            _userbookRepositoryMock
                .Setup(r => r.GetUserbook(userId, bookId))
                .Returns(new Userbook 
                { 
                    UserId = userId,
                    BookId = bookId,
                    Status = StatusBook.QueroLer,
                    Book = mockBook 
                });

            _service.AddBookUser(userId, dto);

            _userbookRepositoryMock.Verify(r => r.AddBookToUser(It.Is<Userbook>(ub => 
                ub.StartDate == null && 
                ub.Status == StatusBook.QueroLer
            )), Times.Once);
        }

        [Test]
        public void AddBookUser_ShouldThrowException_WhenUserAlreadyHasBook()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var dto = new AddBooksUserDTO { BookId = bookId, Status = StatusBook.Lendo };

            _userRepositoryMock.Setup(u => u.Exists(userId)).Returns(true);
            _userbookRepositoryMock.Setup(r => r.BookExists(bookId)).Returns(true);
            
            _userbookRepositoryMock.Setup(r => r.UserHasBook(userId, bookId)).Returns(true);

            var ex = Assert.Throws<ArgumentException>(() => _service.AddBookUser(userId, dto));
            Assert.That(ex.Message, Is.EqualTo("Você já tem esse livro adicionado."));
        }

        // ==========================================
        // TESTS FOR: RemoveBookFromUser
        // ==========================================

        [Test]
        public void RemoveBookFromUser_ShouldRemove_WhenRelationExists()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            _userbookRepositoryMock
                .Setup(repo => repo.DeleteUserBook(userId, bookId))
                .Returns(true);

            Assert.DoesNotThrow(() => _service.RemoveUserBook(userId, bookId));

            _userbookRepositoryMock.Verify(repo => repo.DeleteUserBook(userId, bookId), Times.Once);
        }

        [Test]
        public void RemoveBookFromUser_ShouldThrowException_WhenRelationDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            _userbookRepositoryMock
                .Setup(repo => repo.DeleteUserBook(userId, bookId))
                .Returns(false);

            var exception = Assert.Throws<ArgumentException>(() => _service.RemoveUserBook(userId, bookId));

            Assert.That(exception.Message, Is.EqualTo("Este livro não existe na biblioteca deste usuário"));

            _userbookRepositoryMock.Verify(repo => repo.DeleteUserBook(userId, bookId), Times.Once);
        }
    }
}