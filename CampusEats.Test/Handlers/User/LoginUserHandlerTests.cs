using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Test.Handlers.User;

public class LoginUserHandlerTests
{
    [Fact]
    public async Task Given_ValidCredentials_When_HandleIsCalled_Then_JwtTokenIsReturned()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new Api.Models.User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", Role = Role.Buyer };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(user))
            .Returns("fake-jwt-token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("testuser", "password123");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockJwtService.Verify(service => service.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Given_InvalidUsername_When_HandleIsCalled_Then_UnauthorizedIsReturned()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("invaliduser"))
            .ReturnsAsync((Api.Models.User)null);
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("invaliduser", "password123");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var unauthorized = Assert.IsType<Unauthorized>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);
    }

    [Fact]
    public async Task Given_UserFound_When_HandleIsCalled_Then_TokenGeneratorCalledWithUser()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new Api.Models.User { Id = Guid.NewGuid(), Username = "testuser", Role = Role.Kitchen };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(It.IsAny<Api.Models.User>()))
            .Returns("token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("testuser", "password");

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockJwtService.Verify(service => service.GenerateToken(It.Is<Api.Models.User>(u => u.Id == user.Id)), Times.Once);
    }
}
