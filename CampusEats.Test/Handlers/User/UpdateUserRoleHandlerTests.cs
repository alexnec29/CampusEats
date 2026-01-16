using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class UpdateUserRoleHandlerTests
{
    [Fact]
    public async Task Given_ValidRoleUpdate_When_HandleIsCalled_Then_RoleUpdated()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRoleRequest(userId, "Kitchen");
        var mockUserRepository = new Mock<IUserRepository>();

        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateUserRoleHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<UpdateUserRoleResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Contains("success", okResult.Value!.Message);
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<Api.Models.User>(u => u.Role == Role.Kitchen)), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRoleRequest(userId, "Admin");
        var mockUserRepository = new Mock<IUserRepository>();

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((Api.Models.User?)null);

        var handler = new UpdateUserRoleHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Given_InvalidRole_When_HandleIsCalled_Then_BadRequestReturned()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRoleRequest(userId, "InvalidRole");
        var mockUserRepository = new Mock<IUserRepository>();

        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var handler = new UpdateUserRoleHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("Invalid role", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_AdminRole_When_HandleIsCalled_Then_RoleUpdatedToAdmin()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRoleRequest(userId, "Admin");
        var mockUserRepository = new Mock<IUserRepository>();

        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Kitchen
        };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateUserRoleHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<UpdateUserRoleResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<Api.Models.User>(u => u.Role == Role.Admin)), Times.Once);
    }
}
