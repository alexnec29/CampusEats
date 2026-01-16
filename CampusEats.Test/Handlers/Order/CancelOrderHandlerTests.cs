using CampusEats.Api.Features.Order.CancelOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class CancelOrderHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        CancelOrderRequest request = new CancelOrderRequest(nonExistentOrderId);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        int orderId = 1;
        CancelOrderRequest request = new CancelOrderRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Completed // Cannot cancel completed order
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_PendingOrder_When_HandleIsCalled_Then_OrderCancelledSuccessfully()
    {
        //Arrange
        int orderId = 1;
        CancelOrderRequest request = new CancelOrderRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = result.Should().BeOfType<Ok<Api.Models.Order>>().Subject;
        okResult.Value.Should().NotBeNull();
        okResult.Value.Status.Should().Be(OrderStatus.Cancelled);
        mockedOrderRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_OrderWithKitchenTask_When_Cancelled_Then_KitchenTaskAlsoCancelled()
    {
        //Arrange
        int orderId = 1;
        CancelOrderRequest request = new CancelOrderRequest(orderId);
        
        var kitchenTask = new Api.Models.KitchenTask
        {
            Id = 10,
            OrderId = orderId,
            Status = OrderStatus.Pending,
            CompletedAt = null
        };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            KitchenTask = kitchenTask,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = result.Should().BeOfType<Ok<Api.Models.Order>>().Subject;
        okResult!.Value!.Status.Should().Be(OrderStatus.Cancelled);
        okResult.Value!.KitchenTask!.Status.Should().Be(OrderStatus.Cancelled);
        okResult.Value.KitchenTask.CompletedAt.Should().NotBeNull();
        okResult.Value.KitchenTask.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Given_OrderWithoutKitchenTask_When_Cancelled_Then_OnlyOrderStatusChanged()
    {
        //Arrange
        int orderId = 1;
        CancelOrderRequest request = new CancelOrderRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            KitchenTask = null,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = result.Should().BeOfType<Ok<Api.Models.Order>>().Subject;
        okResult!.Value!.Status.Should().Be(OrderStatus.Cancelled);
        okResult.Value.KitchenTask.Should().BeNull();
    }

    [Theory]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Given_NonPendingStatus_When_HandleIsCalled_Then_BadRequestReturned(OrderStatus status)
    {
        //Arrange
        int orderId = 1;
        CancelOrderRequest request = new CancelOrderRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = status
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
}
