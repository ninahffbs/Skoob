using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using Skoob.Interfaces;
using Skoob.Services;
using Skoob.Models;
using Skoob.DTOs;
using Skoob.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;


[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _repositoryMock;
    private UserService _service;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUserRepository>();
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Pagination:UsersPageSize", "10" }
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

        _service = new UserService(
            _repositoryMock.Object,
            configuration);
    }

    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void Should_Fail_When_Confirmation_Does_Not_Match_NewPassword()
    {
        // Arrange
        var dto = new UpdatePasswordDTO
        {
            OldPassword = "OldPassword123",
            NewPassword = "NewStrong@Password123",
            ConfirmNewPassword = "Diferente@123"
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.That(errors, Is.Not.Empty, "O validador deveria ter retornado erro por senhas não coincidentes.");
    }

    [Test]
    public void GetUsers_WhenRepositoryReturnsEmpty_ShouldReturnEmptyList()
    {
        /// ARRANGE
        _repositoryMock
            .Setup(x => x.SelectUsers(1, 10))
            .Returns(new List<Mainuser>());

        // ACT
        var result = _service.GetUsers(1);

        // ASSERT
        Assert.That(result.Count, Is.EqualTo(0));

        _repositoryMock.Verify(
            x => x.SelectUsers(1, 10),
            Times.Once);
    }

    [Test]
    public void GetUsers_WhenUsersExist_ShouldReturnList()
    {
        /// ARRANGE
        var list = new List<Mainuser>
        {
            new Mainuser
            {
                Id = Guid.NewGuid(),
                UserName = "Nina",
                Email = "nina@email.com",
                CreatedAt = DateTime.UtcNow
            },
            new Mainuser
            {
                Id = Guid.NewGuid(),
                UserName = "Mayra",
                Email = "mayra@email.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        _repositoryMock.Setup(x => x.SelectUsers(1, 10)).Returns(list);

        // ACT
        var result = _service.GetUsers(1);

        // ASSERT
        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].Id, Is.EqualTo(list[0].Id));
        Assert.That(result[0].UserName, Is.EqualTo(list[0].UserName));
        Assert.That(result[0].Email, Is.EqualTo(list[0].Email));

        Assert.That(result[1].Id, Is.EqualTo(list[1].Id));
        Assert.That(result[1].UserName, Is.EqualTo(list[1].UserName));
        Assert.That(result[1].Email, Is.EqualTo(list[1].Email));

        _repositoryMock.Verify(
            x => x.SelectUsers(1, 10),
            Times.Once);
    }
    [Test]
    public void GetUserById_WhenUserDoesNotExist_ShouldReturnNull()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetById(id))
            .Returns((Mainuser?)null);

        var result = _service.GetUserById(id);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetUserById_WhenUserExists_ShouldReturnDTO()
    {
        var user = new Mainuser
        {
            Id = Guid.NewGuid(),
            UserName = "Nina",
            Email = "nina@email.com",
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetById(user.Id))
            .Returns(user);

        var result = _service.GetUserById(user.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo(user.UserName));
        Assert.That(result.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public void GetByUserName_ShouldCalculateBooksCorrectly()
    {
        var user = new Mainuser
        {
            Id = Guid.NewGuid(),
            UserName = "Nina",
            Email = "nina@email.com",
            CreatedAt = DateTime.UtcNow,
            Userbooks = new List<Userbook>
            {
                new Userbook
                {
                    PagesRead = 50,
                    StartDate = DateTime.UtcNow,
                    FinishDate = DateTime.UtcNow,
                    Status = StatusBook.Lido,
                    Book = new Book
                    {
                        Title = "Livro 1",
                        PagesNumber = 100
                    }
                },
                new Userbook
                {
                    PagesRead = 20,
                    StartDate = DateTime.UtcNow,
                    FinishDate = null,
                    Status = StatusBook.Lendo,
                    Book = new Book
                    {
                        Title = "Livro 2",
                        PagesNumber = 200
                    }
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetByName("Nina"))
            .Returns(user);

        var result = _service.GetByUserName("Nina");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalBooks, Is.EqualTo(2));
        Assert.That(result.BooksRead, Is.EqualTo(1));

        // 50 de 100 = 50%
        Assert.That(result.Books[0].PercentComplete, Is.EqualTo(50));
    }

    [Test]
    public void UpdateUserName_WhenUserNotFound_ShouldThrowKeyNotFound()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetById(id))
            .Returns((Mainuser?)null);

        Assert.Throws<KeyNotFoundException>(() =>
            _service.UpdateUserName(id, "NovoNome"));
    }

    [Test]
    public void UpdateUserName_WhenUsernameAlreadyExists_ShouldThrowArgumentException()
    {
        var user = new Mainuser { Id = Guid.NewGuid(), UserName = "Nina" };

        _repositoryMock.Setup(r => r.GetById(user.Id)).Returns(user);
        _repositoryMock.Setup(r => r.UsernameExists("NovoNome")).Returns(true);

        Assert.Throws<ArgumentException>(() =>
            _service.UpdateUserName(user.Id, "NovoNome"));
    }

    [Test]
    public void UpdateUserName_WithValidData_ShouldCallRepository()
    {
        var user = new Mainuser { Id = Guid.NewGuid(), UserName = "Antigo" };

        _repositoryMock.Setup(r => r.GetById(user.Id)).Returns(user);
        _repositoryMock.Setup(r => r.UsernameExists("Novo")).Returns(false);

        _service.UpdateUserName(user.Id, "Novo");

        _repositoryMock.Verify(r => r.UpdateUserName(user), Times.Once);
        Assert.That(user.UserName, Is.EqualTo("Novo"));
    }

    [Test]
    public void CreateUser_WhenUsernameExists_ShouldThrow()
    {
        var dto = new CreateUserDTO
        {
            UserName = "Nina",
            Email = "nina@email.com",
            Password = "123"
        };

        _repositoryMock.Setup(r => r.UsernameExists(dto.UserName)).Returns(true);

        Assert.Throws<ArgumentException>(() => _service.CreateUser(dto));
    }

    [Test]
    public void CreateUser_WithValidData_ShouldReturnDTO()
    {
        var dto = new CreateUserDTO
        {
            UserName = "Nina",
            Email = "nina@email.com",
            Password = "SenhaForte123"
        };

        _repositoryMock.Setup(r => r.UsernameExists(dto.UserName)).Returns(false);
        _repositoryMock.Setup(r => r.EmailExists(dto.Email)).Returns(false);

        _repositoryMock.Setup(r => r.Register(It.IsAny<Mainuser>()))
            .Returns((Mainuser user) =>
            {
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                return user;
            });

        var result = _service.CreateUser(dto);

        Assert.That(result.UserName, Is.EqualTo(dto.UserName));
        Assert.That(result.Email, Is.EqualTo(dto.Email.ToLower()));
    }
    [Test]
    public void UpdatePassword_WhenUserNotFound_ShouldThrow()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetById(id)).Returns((Mainuser?)null);

        Assert.Throws<KeyNotFoundException>(() =>
            _service.UpdatePassword(id, new UpdatePasswordDTO()));
    }
    [Test]
    public void UpdatePassword_WhenOldPasswordIsWrong_ShouldThrow()
    {
        var user = new Mainuser
        {
            Id = Guid.NewGuid(),
            UserPassword = BCrypt.Net.BCrypt.HashPassword("SenhaAntiga")
        };

        _repositoryMock.Setup(r => r.GetById(user.Id)).Returns(user);

        var dto = new UpdatePasswordDTO
        {
            OldPassword = "Errada",
            NewPassword = "NovaSenha123"
        };

        Assert.Throws<ArgumentException>(() =>
            _service.UpdatePassword(user.Id, dto));
    }

    [Test]
    public void UpdatePassword_WithValidData_ShouldCallRepository()
    {
        var user = new Mainuser
        {
            Id = Guid.NewGuid(),
            UserPassword = BCrypt.Net.BCrypt.HashPassword("SenhaAntiga")
        };

        _repositoryMock.Setup(r => r.GetById(user.Id)).Returns(user);

        var dto = new UpdatePasswordDTO
        {
            OldPassword = "SenhaAntiga",
            NewPassword = "NovaSenha123"
        };

        _service.UpdatePassword(user.Id, dto);

        _repositoryMock.Verify(r => r.UpdatePassword(user), Times.Once);
    }

    [Test]
    public void DeleteUser_WhenUserNotFound_ShouldThrow()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetById(id)).Returns((Mainuser?)null);

        Assert.Throws<ArgumentException>(() => _service.DeleteUser(id));
    }

    [Test]
    public void DeleteUser_WhenUserExists_ShouldReturnTrue()
    {
        var user = new Mainuser { Id = Guid.NewGuid() };

        _repositoryMock.Setup(r => r.GetById(user.Id)).Returns(user);
        _repositoryMock.Setup(r => r.DeleteUser(user.Id)).Returns(true);

        var result = _service.DeleteUser(user.Id);

        Assert.That(result, Is.True);
    }

    [Test]
    public void UpdateUserName_WhenDbUniqueViolation_ShouldThrowArgumentException()
    {
        var userId = Guid.NewGuid();
        var newName = "novoNome";

        var user = new Mainuser
        {
            Id = userId,
            UserName = "oldName"
        };

        _repositoryMock.Setup(x => x.GetById(userId))
            .Returns(user);

        _repositoryMock.Setup(x => x.UsernameExists(newName))
            .Returns(false);

        // Simula erro de unique constraint do PostgreSQL
        var postgresException = new PostgresException(
            messageText: "duplicate key",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: "23505");

        var dbUpdateException = new DbUpdateException(
            "Erro",
            postgresException);

        _repositoryMock
            .Setup(x => x.UpdateUserName(user))
            .Throws(dbUpdateException);

        var ex = Assert.Throws<ArgumentException>(() =>
            _service.UpdateUserName(userId, newName));

        Assert.That(ex.Message,
            Is.EqualTo($"Nome de usuário '{newName}' já está em uso"));
    }

    [Test]
    public void CreateUser_WhenEmailAlreadyExists_ShouldThrowArgumentException()
    {
        var dto = new CreateUserDTO
        {
            UserName = "usuario",
            Email = "teste@email.com",
            Password = "123456"
        };

        _repositoryMock.Setup(x => x.UsernameExists(dto.UserName))
            .Returns(false);

        _repositoryMock.Setup(x => x.EmailExists(dto.Email))
            .Returns(true);

        var ex = Assert.Throws<ArgumentException>(() =>
            _service.CreateUser(dto));

        Assert.That(ex.Message,
            Is.EqualTo($"Email '{dto.Email}' já está cadastrado"));
    }
    [Test]
    public void GetByUserName_WhenUserHasNoBooks_ShouldReturnDTOWithZeroBooks()
    {
        // ARRANGE
        var user = new Mainuser
        {
            Id = Guid.NewGuid(),
            UserName = "UsuarioSemLivros",
            Email = "vazio@email.com",
            CreatedAt = DateTime.UtcNow,
            Userbooks = new List<Userbook>() // Lista vazia para testar a contagem zero
        };

        _repositoryMock
            .Setup(r => r.GetByName("UsuarioSemLivros"))
            .Returns(user);

        // ACT
        var result = _service.GetByUserName("UsuarioSemLivros");

        // ASSERT
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalBooks, Is.EqualTo(0));
        Assert.That(result.BooksRead, Is.EqualTo(0));
        Assert.That(result.Books, Is.Empty);

        _repositoryMock.Verify(r => r.GetByName("UsuarioSemLivros"), Times.Once);
    }

    [Test]
    public void GetByUserName_WhenUserNotFound_ShouldReturnNull()
    {
        // ARRANGE
        _repositoryMock
            .Setup(r => r.GetByName("Inexistente"))
            .Returns((Mainuser?)null);

        // ACT
        var result = _service.GetByUserName("Inexistente");

        // ASSERT
        Assert.That(result, Is.Null);
    }
    [Test]
    public void GetUsers_WithBooks_ShouldMapBooksReadAndTitlesCorrectly()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var list = new List<Mainuser>
        {
            new Mainuser
            {
                Id = userId,
                UserName = "Nina",
                Email = "nina@email.com",
                CreatedAt = DateTime.UtcNow,
                Userbooks = new List<Userbook>
                {
                    new Userbook 
                    { 
                        FinishDate = DateTime.UtcNow, // Este conta no BooksRead
                        Book = new Book { Title = "Livro Lido" } 
                    },
                    new Userbook 
                    { 
                        FinishDate = null, // Este NÃO conta no BooksRead
                        Book = new Book { Title = "Livro Lendo" } 
                    }
                }
            }
        };

        _repositoryMock.Setup(x => x.SelectUsers(1, 10)).Returns(list);

        // ACT
        var result = _service.GetUsers(1);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(result.Count, Is.EqualTo(1));
            // Testa a linha: BooksRead = user.Userbooks?.Count(ub => ub.FinishDate != null)
            Assert.That(result[0].BooksRead, Is.EqualTo(1), "Deveria ter contado apenas 1 livro lido.");
            
            // Testa a linha: Books = user.Userbooks?.Select(ub => ub.Book.Title).ToList()
            Assert.That(result[0].Books, Contains.Item("Livro Lido"));
            Assert.That(result[0].Books, Contains.Item("Livro Lendo"));
            Assert.That(result[0].Books!.Count, Is.EqualTo(2));
        });

        _repositoryMock.Verify(x => x.SelectUsers(1, 10), Times.Once);
    }
}
