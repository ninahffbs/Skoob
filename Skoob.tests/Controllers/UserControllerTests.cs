using Moq;
using NUnit.Framework;
using Skoob.Controllers;
using Skoob.Interfaces;
using Skoob.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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

    // ==========================================================
    // GetAll
    // ==========================================================

    [Test]
    public void GetAll_UsersExist_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<UserResponseDTO>
        {
            new UserResponseDTO { Id = Guid.NewGuid(), UserName = "User1" }
        };

        _mockService.Setup(s => s.GetUsers(1)).Returns(users);

        // Act
        var result = _controller.GetAll(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));

        _mockService.Verify(s => s.GetUsers(1), Times.Once);
    }

    // ==========================================================
    // GetById
    // ==========================================================

    [Test]
    public void GetById_UserExists_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var user = new UserResponseDTO { Id = id };

        _mockService.Setup(s => s.GetUserById(id)).Returns(user);

        var result = _controller.GetById(id);

        var ok = result.Result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.StatusCode, Is.EqualTo(200));
        _mockService.Verify(s => s.GetUserById(id), Times.Once);
    }

    [Test]
    public void GetById_UserDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.GetUserById(id)).Returns((UserResponseDTO)null);

        var result = _controller.GetById(id);

        var notFound = result.Result as NotFoundObjectResult;

        Assert.That(notFound, Is.Not.Null);
        Assert.That(notFound.StatusCode, Is.EqualTo(404));
        _mockService.Verify(s => s.GetUserById(id), Times.Once);
    }

    // ==========================================================
    // GetProfile
    // ==========================================================

    [Test]
    public void GetProfile_UserExists_ReturnsOk()
    {
        var username = "user123";
        var user = new UserProfileDTO { UserName = username };

        _mockService.Setup(s => s.GetByUserName(username)).Returns(user);

        var result = _controller.GetProfile(username);

        var ok = result.Result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        _mockService.Verify(s => s.GetByUserName(username), Times.Once);
    }

    [Test]
    public void GetProfile_UserNotFound_ReturnsNotFound()
    {
        var username = "user123";

        _mockService.Setup(s => s.GetByUserName(username)).Returns((UserProfileDTO)null);

        var result = _controller.GetProfile(username);

        var notFound = result.Result as NotFoundObjectResult;

        Assert.That(notFound, Is.Not.Null);
        _mockService.Verify(s => s.GetByUserName(username), Times.Once);
    }

    // ==========================================================
    // UpdateUserName
    // ==========================================================

    [Test]
    public void UpdateUserName_ValidRequest_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new UpdateUserNameRequest { UserName = "newUser" };

        var result = _controller.UpdateUserName(id, request);

        var ok = result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.StatusCode, Is.EqualTo(200));
        _mockService.Verify(s => s.UpdateUserName(id, request.UserName), Times.Once);
    }

    [Test]
    public void UpdateUserName_UserNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var request = new UpdateUserNameRequest { UserName = "newUser" };

        _mockService
            .Setup(s => s.UpdateUserName(id, request.UserName))
            .Throws(new KeyNotFoundException("Usuário não encontrado"));

        var result = _controller.UpdateUserName(id, request);

        var notFound = result as NotFoundObjectResult;

        Assert.That(notFound, Is.Not.Null);
        _mockService.Verify(s => s.UpdateUserName(id, request.UserName), Times.Once);
    }

    [Test]
    public void UpdateUserName_Conflict_ReturnsConflict()
    {
        var id = Guid.NewGuid();
        var request = new UpdateUserNameRequest { UserName = "newUser" };

        _mockService
            .Setup(s => s.UpdateUserName(id, request.UserName))
            .Throws(new ArgumentException("Username já existe"));

        var result = _controller.UpdateUserName(id, request);

        var conflict = result as ConflictObjectResult;

        Assert.That(conflict, Is.Not.Null);
        _mockService.Verify(s => s.UpdateUserName(id, request.UserName), Times.Once);
    }

    // ==========================================================
    // Create
    // ==========================================================

    [Test]
    public void Create_ValidData_ReturnsOk()
    {
        var dto = new CreateUserDTO
        {
            UserName = "user123",
            Email = "user@email.com",
            Password = "Password@1",
            ConfirmPassword = "Password@1"
        };

        var user = new UserResponseDTO { UserName = dto.UserName };

        _mockService.Setup(s => s.CreateUser(dto)).Returns(user);

        var result = _controller.Create(dto);

        var ok = result.Result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        _mockService.Verify(s => s.CreateUser(dto), Times.Once);
    }

    [Test]
    public void Create_InvalidData_ReturnsBadRequest()
    {
        var dto = new CreateUserDTO();

        _mockService
            .Setup(s => s.CreateUser(dto))
            .Throws(new ArgumentException("Erro de validação"));

        var result = _controller.Create(dto);

        var badRequest = result.Result as BadRequestObjectResult;

        Assert.That(badRequest, Is.Not.Null);
        _mockService.Verify(s => s.CreateUser(dto), Times.Once);
    }

    // ==========================================================
    // UpdatePassword
    // ==========================================================

    [Test]
    public void UpdatePassword_Valid_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = new UpdatePasswordDTO
        {
            OldPassword = "Old@1234",
            NewPassword = "New@1234A",
            ConfirmNewPassword = "New@1234A"
        };

        var result = _controller.UpdatePassword(id, dto);

        var ok = result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        _mockService.Verify(s => s.UpdatePassword(id, dto), Times.Once);
    }

    [Test]
    public void UpdatePassword_UserNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var dto = new UpdatePasswordDTO();

        _mockService
            .Setup(s => s.UpdatePassword(id, dto))
            .Throws(new KeyNotFoundException("Usuário não encontrado"));

        var result = _controller.UpdatePassword(id, dto);

        var notFound = result as NotFoundObjectResult;

        Assert.That(notFound, Is.Not.Null);
        _mockService.Verify(s => s.UpdatePassword(id, dto), Times.Once);
    }

    [Test]
    public void UpdatePassword_InvalidData_ReturnsConflict()
    {
        var id = Guid.NewGuid();
        var dto = new UpdatePasswordDTO();

        _mockService
            .Setup(s => s.UpdatePassword(id, dto))
            .Throws(new ArgumentException("Erro"));

        var result = _controller.UpdatePassword(id, dto);

        var conflict = result as ConflictObjectResult;

        Assert.That(conflict, Is.Not.Null);
        _mockService.Verify(s => s.UpdatePassword(id, dto), Times.Once);
    }

    // ==========================================================
    // Delete
    // ==========================================================

    [Test]
    public void Delete_UserExists_ReturnsOk()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.DeleteUser(id)).Returns(true);

        var result = _controller.Delete(id);

        var ok = result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        _mockService.Verify(s => s.DeleteUser(id), Times.Once);
    }

    [Test]
    public void Delete_UserNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.DeleteUser(id)).Returns(false);

        var result = _controller.Delete(id);

        var notFound = result as NotFoundObjectResult;

        Assert.That(notFound, Is.Not.Null);
        _mockService.Verify(s => s.DeleteUser(id), Times.Once);
    }
}
