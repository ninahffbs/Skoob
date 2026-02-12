using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Skoob.Interfaces;
using Skoob.Services;
using Skoob.Models;
using Skoob.DTOs;

[TestFixture]
public class UserServiceTest
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
}