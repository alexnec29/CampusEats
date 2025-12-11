using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.User;

public class GetUserByIdHandlerTests
{
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_UserIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User { Id = userId, Username = "testuser", Email = "test@email.com" };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@email.com");
    }

    [Fact]
    public async Task Given_NonExistentUserId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((Api.Models.User)null);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_UserId_When_HandleIsCalled_Then_RepositoryCalledWithCorrectId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User { Id = userId };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }
}
