using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserModel = CampusEats.Api.Models.User;

namespace CampusEats.Test.Handlers;

public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new CreateUserHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesUserSuccessfully()
    {
        // Arrange
        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "newuser@example.com",
            Password: "SecurePassword123!",
            ConfirmPassword: "SecurePassword123!"
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<UserModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<UserModel>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ReturnsConflict()
    {
        // Arrange
        var existingUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "existing@example.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        var request = new CreateUserRequest(
            Username: "existinguser",
            Email: "newuser@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!"
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsConflict()
    {
        // Arrange
        var existingUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "differentuser",
            Email = "existing@example.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "existing@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!"
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Handle_WithMismatchedPasswords_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "newuser@example.com",
            Password: "Password123!",
            ConfirmPassword: "DifferentPassword123!"
        );

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((UserModel?)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesUserWithBuyerRole()
    {
        // Arrange
        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "newuser@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!"
        );

        UserModel? capturedUser = null;

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<UserModel>()))
            .Callback<UserModel>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser?.Role.Should().Be(Role.Buyer);
    }

    [Fact]
    public async Task Handle_WithValidRequest_HashedPasswordIsNotPlaintext()
    {
        // Arrange
        var password = "PlaintextPassword123!";
        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "newuser@example.com",
            Password: password,
            ConfirmPassword: password
        );

        UserModel? capturedUser = null;

        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((UserModel?)null);

        _mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<UserModel>()))
            .Callback<UserModel>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        capturedUser?.HashedPassword.Should().NotBe(password);
    }
}
