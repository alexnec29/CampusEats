using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.User;

public class GetBuyerProfileByUserIdHandlerTests
{
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_BuyerProfileIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var buyerProfile = new Api.Models.BuyerProfile 
        { 
            UserId = userId, 
            PhoneNumber = "0712345678",
            PreferredPaymentMethod = "CreditCard"
        };
        
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(buyerProfile);
        
        var handler = new GetBuyerProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetBuyerProfileByUserIdRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PhoneNumber.Should().Be("0712345678");
    }

    [Fact]
    public async Task Given_NonExistentUserId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync((Api.Models.BuyerProfile)null);
        
        var handler = new GetBuyerProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetBuyerProfileByUserIdRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
