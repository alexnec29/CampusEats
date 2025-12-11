using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Validators;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.User;

public class UpdateBuyerProfileHandlerTests
{
    [Fact]
    public async Task Given_ValidBuyerProfileUpdate_When_HandleIsCalled_Then_ProfileIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateBuyerProfileValidator>();
        
        var existingProfile = new Api.Models.BuyerProfile { UserId = userId, PhoneNumber = "0712345678" };
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(existingProfile);
        
        var handler = new UpdateBuyerProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateBuyerProfileRequest(userId, "0798765432", "NewStreet 123", "CreditCard");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        existingProfile.PhoneNumber.Should().Be("0798765432");
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentBuyerProfile_When_HandleIsCalled_Then_NothingIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateBuyerProfileValidator>();
        
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync((Api.Models.BuyerProfile)null);
        
        var handler = new UpdateBuyerProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateBuyerProfileRequest(userId, "0798765432", "Street", "CreditCard");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }
}
