using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Validators;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.User;

public class UpdateKitchenProfileHandlerTests
{
    [Fact]
    public async Task Given_ValidKitchenProfileUpdate_When_HandleIsCalled_Then_ProfileIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateKitchenProfileValidator>();
        
        var existingProfile = new Api.Models.KitchenProfile 
        { 
            UserId = userId, 
            RestaurantName = "Old Kitchen",
            Description = "Old description"
        };
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(existingProfile);
        
        var handler = new UpdateKitchenProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateKitchenProfileRequest(userId, "New Kitchen", "New description", "CuisineType");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        existingProfile.RestaurantName.Should().Be("New Kitchen");
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentKitchenProfile_When_HandleIsCalled_Then_NothingIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateKitchenProfileValidator>();
        
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync((Api.Models.KitchenProfile)null);
        
        var handler = new UpdateKitchenProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateKitchenProfileRequest(userId, "Kitchen", "Description", "Cuisine");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Fact]
    public async Task Given_ValidProfile_When_HandleIsCalled_Then_RepositoryCalledOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateKitchenProfileValidator>();
        
        var profile = new Api.Models.KitchenProfile { UserId = userId };
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new UpdateKitchenProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateKitchenProfileRequest(userId, "Kitchen", "Desc", "Cuisine");

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }
}
