using CampusEats.Api.Features.Order.GetOrderById;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Order;

public class GetOrderByIdHandlerTests
{
    [Fact]
    public async Task Given_ValidOrderId_When_HandleIsCalled_Then_OrderIsReturned()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new GetOrderByIdHandler(mockOrderRepository.Object);
        var request = new GetOrderByIdRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentOrderId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new GetOrderByIdHandler(mockOrderRepository.Object);
        var request = new GetOrderByIdRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
