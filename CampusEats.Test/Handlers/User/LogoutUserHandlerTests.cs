using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Test.Handlers.User;

public class LogoutUserHandlerTests
{
    [Fact]
    public async Task Given_ValidLogoutRequest_When_HandleIsCalled_Then_SuccessIsReturned()
    {
        // Arrange
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        
        var handler = new LogoutUserHandler(mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest("fake-jwt-token");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockBlacklistRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.BlackListToken>()), Times.Once);
    }

    [Fact]
    public async Task Given_ValidToken_When_HandleIsCalled_Then_BlacklistIsUpdated()
    {
        // Arrange
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        var token = "test-jwt-token";
        
        var handler = new LogoutUserHandler(mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest(token);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockBlacklistRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.BlackListToken>(t => t.Token == token)),
            Times.Once);
    }

    [Fact]
    public async Task Given_EmptyToken_When_HandleIsCalled_Then_ValidatorThrowsError()
    {
        // Arrange
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<LogoutUserRequest>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        
        var handler = new LogoutUserHandler(mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest("");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }
}
