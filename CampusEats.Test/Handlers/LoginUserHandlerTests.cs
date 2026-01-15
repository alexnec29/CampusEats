using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.JwtUtil;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserModel = CampusEats.Api.Models.User;

namespace CampusEats.Test.Handlers;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService<UserModel>> _mockJwtService;
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService<UserModel>>();
        _handler = new LoginUserHandler(_mockUserRepository.Object, _mockJwtService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var testUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = hashedPassword,
            Role = Role.Buyer
        };

        var request = new LoginUserRequest(
            Username: "testuser",
            Password: password
        );

        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(testUser);

        _mockJwtService
            .Setup(s => s.GenerateToken(testUser))
            .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockJwtService.Verify(s => s.GenerateToken(testUser), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUsername_ReturnsNotFound()
    {
        // Arrange
        var request = new LoginUserRequest(
            Username: "nonexistentuser",
            Password: "AnyPassword123!"
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((UserModel?)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        var testUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
            Role = Role.Buyer
        };

        var request = new LoginUserRequest(
            Username: "testuser",
            Password: "WrongPassword123!"
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(testUser);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_GeneratesTokenOnce()
    {
        // Arrange
        var password = "TestPassword123!";
        var testUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Role.Buyer
        };

        var request = new LoginUserRequest(
            Username: "testuser",
            Password: password
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(testUser);

        _mockJwtService
            .Setup(s => s.GenerateToken(It.IsAny<UserModel>()))
            .Returns("token");

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _mockJwtService.Verify(s => s.GenerateToken(It.IsAny<UserModel>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_GeneratesTokenForCorrectUser()
    {
        // Arrange
        var password = "TestPassword123!";
        var userId = Guid.NewGuid();
        var testUser = new UserModel
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Role.Buyer
        };

        var request = new LoginUserRequest(
            Username: "testuser",
            Password: password
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(testUser);

        _mockJwtService
            .Setup(s => s.GenerateToken(It.Is<UserModel>(u => u.Id == userId)))
            .Returns("token");

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _mockJwtService.Verify(s => s.GenerateToken(It.Is<UserModel>(u => u.Id == userId)), Times.Once);
    }
}
