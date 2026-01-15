using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class ChangePasswordHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new ChangePasswordHandler(_mockUserRepository.Object);
    }

    [Theory]
    [InlineData("currentPassword123", "newPassword456", "newPassword456")]
    [InlineData("oldPass", "newPass123", "newPass123")]
    public async Task Handle_WithValidPasswords_ShouldUpdatePasswordSuccessfully(
        string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = Guid.NewGuid();
        var hashedCurrentPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = hashedCurrentPassword,
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new ChangePasswordRequest(currentPassword, newPassword, confirmPassword)
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }

    [Theory]
    [InlineData("", "newPassword", "newPassword")]
    [InlineData("currentPassword", "", "")]
    [InlineData("currentPassword", "newPassword", "")]
    public async Task Handle_WithEmptyFields_ShouldReturnBadRequest(
        string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest(currentPassword, newPassword, confirmPassword)
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Theory]
    [InlineData("newPassword123", "differentPassword456")]
    [InlineData("pass1", "pass2")]
    public async Task Handle_WithMismatchedPasswords_ShouldReturnBadRequest(
        string newPassword, string confirmPassword)
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("currentPassword", newPassword, confirmPassword)
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("abcd")]
    [InlineData("abcde")]
    public async Task Handle_WithShortPassword_ShouldReturnBadRequest(string shortPassword)
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("currentPassword", shortPassword, shortPassword)
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Api.Models.User?)null);

        var request = new ChangePasswordRequest("currentPassword", "newPassword123", "newPassword123")
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Theory]
    [InlineData("wrongPassword")]
    [InlineData("incorrectPassword")]
    [InlineData("notTheRightPassword")]
    public async Task Handle_WithIncorrectCurrentPassword_ShouldReturnBadRequest(string wrongPassword)
    {
        var userId = Guid.NewGuid();
        var hashedCurrentPassword = BCrypt.Net.BCrypt.HashPassword("correctPassword");
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = hashedCurrentPassword,
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new ChangePasswordRequest(wrongPassword, "newPassword123", "newPassword123")
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("abcdef")]
    [InlineData("password123")]
    [InlineData("longPasswordWithMoreCharacters")]
    public async Task Handle_WithValidLengthPasswords_ShouldSucceed(string validPassword)
    {
        var userId = Guid.NewGuid();
        var hashedCurrentPassword = BCrypt.Net.BCrypt.HashPassword("currentPassword");
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = hashedCurrentPassword,
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new ChangePasswordRequest("currentPassword", validPassword, validPassword)
        {
            UserId = userId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        var userId = Guid.NewGuid();
        var currentPassword = "currentPassword";
        var newPassword = "newPassword123";
        var hashedCurrentPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = hashedCurrentPassword,
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new ChangePasswordRequest(currentPassword, newPassword, newPassword)
        {
            UserId = userId
        };

        await _handler.Handle(request, CancellationToken.None);

        user.HashedPassword.Should().NotBe(newPassword);
        BCrypt.Net.BCrypt.Verify(newPassword, user.HashedPassword).Should().BeTrue();
    }
}
