using Moq;
using Skoob.Controllers;
using Skoob.Interfaces;
using Skoob.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Skoob.tests;


[TestFixture]
public class UserControllerTests
{
    private Mock<IUserService> _mockService;
    private UserController _controller;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UserController(_mockService.Object);
    }

    [Test]
    public void Should_return_Sucess_When_User_Exists()
    {
        var userId = Guid.NewGuid();
            var fakeUser = new UserResponseDTO { Id = userId, UserName = "Mayyzena Teste" };
        _mockService.Setup(s => s.GetUserById(userId)).Returns(fakeUser);

        // ACT
        var test = _controller.GetById(userId);

        // ASSERT
        var okResult = test.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));

        var returnValue = okResult.Value as UserResponseDTO;
        Assert.That(returnValue?.UserName, Is.EqualTo("Mayyzena Teste"));
    }

    [Test]
        public void Should_Fail_When_User_Not_Found()
        {
            var userId = Guid.NewGuid();

            _mockService.Setup(s => s.GetUserById(userId)).Returns((UserResponseDTO)null);

            var resultFail = _controller.GetById(userId);

            var notFoundResult = resultFail.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        }
}    