using CampusEats.Api.Features.Order.UpdateOrderItemQuantity;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class UpdateOrderItemQuantityHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(nonExistentOrderId, 1, 5);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonExistentOrderItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int orderId = 1;
        int nonExistentItemId = 999;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(orderId, nonExistentItemId, 5);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(3, 15.50, 46.50)]
    [InlineData(5, 7.99, 39.95)]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_QuantityUpdatedSuccessfully(int newQuantity, decimal price, decimal expectedTotal)
    {
        //Arrange
        int orderId = 1;
        int orderItemId = 5;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(orderId, orderItemId, newQuantity);
        
        var orderItem = new Api.Models.OrderItem
        {
            Id = orderItemId,
            MenuItemId = 10,
            Quantity = 2,
            Price = price
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
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        orderItem.Quantity.Should().Be(newQuantity);
        order.TotalAmount.Should().Be(expectedTotal);
        mockedOrderRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleItems_When_OneUpdated_Then_TotalRecalculatedCorrectly()
    {
        //Arrange
        int orderId = 1;
        int itemToUpdateId = 5;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(orderId, itemToUpdateId, 10);
        
        var item1 = new Api.Models.OrderItem { Id = itemToUpdateId, MenuItemId = 10, Quantity = 2, Price = 10.00m };
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
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        item1.Quantity.Should().Be(10);
        order.TotalAmount.Should().Be(130.00m); // (10*10) + 15 + (3*5)
    }

    [Theory]
    [InlineData(2, 5, 10.00, 50.00)]
    [InlineData(10, 1, 12.50, 12.50)]
    [InlineData(1, 20, 3.99, 79.80)]
    public async Task Given_QuantityChange_When_HandleIsCalled_Then_TotalUpdatedCorrectly(int oldQty, int newQty, decimal price, decimal expectedTotal)
    {
        //Arrange
        int orderId = 1;
        int orderItemId = 5;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(orderId, orderItemId, newQty);
        
        var orderItem = new Api.Models.OrderItem
        {
            Id = orderItemId,
            MenuItemId = 10,
            Quantity = oldQty,
            Price = price
        };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem> { orderItem },
            TotalAmount = oldQty * price
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        orderItem.Quantity.Should().Be(newQty);
        order.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task Given_OrderWithThreeItems_When_MiddleItemUpdated_Then_OnlyTargetItemChanged()
    {
        //Arrange
        int orderId = 1;
        int itemToUpdateId = 6;
        int newQuantity = 100;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(orderId, itemToUpdateId, newQuantity);
        
        var item1 = new Api.Models.OrderItem { Id = 5, MenuItemId = 10, Quantity = 2, Price = 10.00m };
        var item2 = new Api.Models.OrderItem { Id = itemToUpdateId, MenuItemId = 11, Quantity = 1, Price = 15.00m };
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
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Should().BeOfType<Ok>();
        item1.Quantity.Should().Be(2); // Unchanged
        item2.Quantity.Should().Be(newQuantity); // Changed
        item3.Quantity.Should().Be(3); // Unchanged
        order.TotalAmount.Should().Be(1535.00m); // (2*10) + (100*15) + (3*5)
    }
}
