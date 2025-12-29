using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.API.Controllers;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Persistence;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using Xunit;

namespace UserService.Tests;

public class UsersControllerTests
{
    [Fact]
    public async Task GetById_Forbids_When_Not_Owner()
    {
        var repoMock = new Mock<IUserRepository>();
        var authMock = new Mock<IAuthService>();

        var userId = Guid.NewGuid();
        authMock.Setup(a => a.GetCurrentUserId()).Returns(Guid.NewGuid().ToString()); // different user

        var controller = new UsersController(repoMock.Object, authMock.Object);

        var result = await controller.GetById(userId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetById_Returns_Ok_For_Owner()
    {
        var repoMock = new Mock<IUserRepository>();
        var authMock = new Mock<IAuthService>();

        var userId = Guid.NewGuid();
        authMock.Setup(a => a.GetCurrentUserId()).Returns(userId.ToString());

        var dto = new UserService.Application.DTOs.UserDto(userId, "Name", "x@y.com");

        repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(dto);

        var controller = new UsersController(repoMock.Object, authMock.Object);

        var result = await controller.GetById(userId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
