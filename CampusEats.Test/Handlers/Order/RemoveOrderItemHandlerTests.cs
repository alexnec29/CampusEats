using CampusEats.Api.Features.Order.RemoveOrderItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class RemoveOrderItemHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(nonExistentOrderId, 1);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
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
        int orderItemId = 5;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, orderItemId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Placed // Cannot remove items from placed order
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_NonExistentOrderItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int orderId = 1;
        int nonExistentItemId = 999;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, nonExistentItemId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_ValidOrderItem_When_HandleIsCalled_Then_ItemRemovedSuccessfully()
    {
        //Arrange
        int orderId = 1;
        int orderItemId = 5;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, orderItemId);
        
        var orderItem = new Api.Models.OrderItem
        {
            Id = orderItemId,
            MenuItemId = 10,
            Quantity = 2,
            Price = 10.00m
        };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem> { orderItem },
            TotalAmount = 20.00m
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        order.OrderItems.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
        mockedOrderRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleItems_When_OneRemoved_Then_TotalRecalculatedCorrectly()
    {
        //Arrange
        int orderId = 1;
        int itemToRemoveId = 5;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, itemToRemoveId);
        
        var item1 = new Api.Models.OrderItem { Id = itemToRemoveId, MenuItemId = 10, Quantity = 2, Price = 10.00m };
        var item2 = new Api.Models.OrderItem { Id = 6, MenuItemId = 11, Quantity = 1, Price = 15.00m };
        var item3 = new Api.Models.OrderItem { Id = 7, MenuItemId = 12, Quantity = 3, Price = 5.00m };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem> { item1, item2, item3 },
            TotalAmount = 50.00m
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        order.OrderItems.Should().HaveCount(2);
        order.OrderItems.Should().NotContain(item1);
        order.TotalAmount.Should().Be(30.00m); // 15 + 15
    }

    [Theory]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Given_NonPendingOrderStatus_When_HandleIsCalled_Then_BadRequestReturned(OrderStatus status)
    {
        //Arrange
        int orderId = 1;
        int orderItemId = 5;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, orderItemId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = status
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_LastItemRemoved_When_HandleIsCalled_Then_OrderTotalIsZero()
    {
        //Arrange
        int orderId = 1;
        int orderItemId = 5;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, orderItemId);
        
        var orderItem = new Api.Models.OrderItem
        {
            Id = orderItemId,
            MenuItemId = 10,
            Quantity = 5,
            Price = 12.50m
        };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem> { orderItem },
            TotalAmount = 62.50m
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        order.OrderItems.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
    }
}
