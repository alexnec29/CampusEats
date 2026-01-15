using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class ChangePasswordHandlerTests
{
    [Fact]
    public async Task Given_ValidPasswordChange_When_HandleIsCalled_Then_PasswordUpdated()
    {
        var userId = Guid.NewGuid();
        var currentPassword = "OldPassword123";
        var newPassword = "NewPassword456";
        var hashedOldPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var request = new ChangePasswordRequest(currentPassword, newPassword, newPassword) { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = hashedOldPassword,
            Role = Role.Buyer
        };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()))
            .Returns(Task.CompletedTask);

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<object>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }

    [Fact]
    public async Task Given_EmptyCurrentPassword_When_HandleIsCalled_Then_BadRequestReturned()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("", "NewPassword123", "NewPassword123") { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("required", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_EmptyNewPassword_When_HandleIsCalled_Then_BadRequestReturned()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldPassword123", "", "NewPassword123") { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("required", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_MismatchedPasswordConfirmation_When_HandleIsCalled_Then_BadRequestReturned()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldPassword123", "NewPassword123", "DifferentPassword") { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("do not match", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_ShortNewPassword_When_HandleIsCalled_Then_BadRequestReturned()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldPassword123", "short", "short") { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("at least 6 characters", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldPassword123", "NewPassword456", "NewPassword456") { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((Api.Models.User?)null);

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Given_IncorrectCurrentPassword_When_HandleIsCalled_Then_BadRequestReturned()
    {
        var userId = Guid.NewGuid();
        var currentPassword = "OldPassword123";
        var incorrectPassword = "WrongPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var request = new ChangePasswordRequest(incorrectPassword, "NewPassword456", "NewPassword456") { UserId = userId };
        var mockUserRepository = new Mock<IUserRepository>();

        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = hashedPassword,
            Role = Role.Buyer
        };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var handler = new ChangePasswordHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("incorrect", badRequestResult.Value);
    }
}
