using CampusEats.Api.Features.Order.CancelOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Order;

public class CancelOrderHandlerTests
{
    [Fact]
    public async Task Given_PendingOrder_When_HandleIsCalled_Then_OrderIsCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CancelOrderValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.Pending };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CancelOrderHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new CancelOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        mockOrderRepository.Verify(repo => repo.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundIsReturned()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CancelOrderValidator>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new CancelOrderHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new CancelOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFound>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Given_CompletedOrder_When_HandleIsCalled_Then_BadRequestIsReturned()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CancelOrderValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.Completed };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CancelOrderHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new CancelOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }
}
