using CampusEats.Api.Features.Order.GetUserOrders;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Order;

public class GetUserOrdersHandlerTests
{
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_UserOrdersReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 50m },
            new Api.Models.Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 75m }
        };
        
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        var handler = new GetUserOrdersHandler(mockOrderRepository.Object);
        var request = new GetUserOrdersRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        mockOrderRepository.Verify(repo => repo.GetOrdersByUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Given_UserWithoutOrders_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Api.Models.Order>());
        
        var handler = new GetUserOrdersHandler(mockOrderRepository.Object);
        var request = new GetUserOrdersRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_OnlyUserOrdersReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var userOrders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), UserId = userId }
        };
        
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(userOrders);
        
        var handler = new GetUserOrdersHandler(mockOrderRepository.Object);
        var request = new GetUserOrdersRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.All(o => o.UserId == userId).Should().BeTrue();
    }
}
