using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.User;

public class GetKitchenProfileByUserIdHandlerTests
{
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_KitchenProfileIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var kitchenProfile = new Api.Models.KitchenProfile 
        { 
            UserId = userId, 
            RestaurantName = "Test Kitchen",
            Description = "A test kitchen"
        };
        
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(kitchenProfile);
        
        var handler = new GetKitchenProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetKitchenProfileByUserIdRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RestaurantName.Should().Be("Test Kitchen");
    }

    [Fact]
    public async Task Given_NonExistentUserId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync((Api.Models.KitchenProfile)null);
        
        var handler = new GetKitchenProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetKitchenProfileByUserIdRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_RepositoryCalledOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var kitchenProfile = new Api.Models.KitchenProfile { UserId = userId };
        
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(kitchenProfile);
        
        var handler = new GetKitchenProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetKitchenProfileByUserIdRequest(userId);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockUserRepository.Verify(repo => repo.GetKitchenProfileAsync(userId), Times.Once);
    }
}
