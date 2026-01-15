using CampusEats.Api.Features.Order.AddOrderItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class AddOrderItemHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        int nonExistentOrderId = 999;
        AddOrderItemRequest request = new AddOrderItemRequest(nonExistentOrderId, 1, 2);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Theory]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned(OrderStatus status)
    {
        int orderId = 1;
        AddOrderItemRequest request = new AddOrderItemRequest(orderId, 1, 2);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = status,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_NonExistentMenuItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        int orderId = 1;
        int nonExistentMenuItemId = 999;
        AddOrderItemRequest request = new AddOrderItemRequest(orderId, nonExistentMenuItemId, 2);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedMenuItemRepo.Setup(r => r.GetByIdAsync(nonExistentMenuItemId))
            .ReturnsAsync((Api.Models.MenuItem?)null);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var notFoundResult = result.Should().BeOfType<NotFound<string>>().Subject;
        notFoundResult.Value.Should().Contain("Menu item not found");
    }

    [Theory]
    [InlineData(1, 10.00)]
    [InlineData(2, 15.50)]
    [InlineData(5, 7.99)]
    public async Task Given_NewMenuItem_When_HandleIsCalled_Then_ItemAddedToOrder(int quantity, decimal price)
    {
        int orderId = 1;
        int menuItemId = 10;
        AddOrderItemRequest request = new AddOrderItemRequest(orderId, menuItemId, quantity);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>(),
            TotalAmount = 0
        };
        
        var menuItem = new Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Price = price
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedMenuItemRepo.Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync(menuItem);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        result.Should().BeOfType<Ok>();
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().MenuItemId.Should().Be(menuItemId);
        order.OrderItems.First().Quantity.Should().Be(quantity);
        order.OrderItems.First().Price.Should().Be(price);
        order.TotalAmount.Should().Be(price * quantity);
    }

    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(1, 1, 2)]
    [InlineData(5, 10, 15)]
    public async Task Given_ExistingMenuItem_When_HandleIsCalled_Then_QuantityIncreased(int existingQty, int addedQty, int expectedQty)
    {
        int orderId = 1;
        int menuItemId = 10;
        decimal price = 10.00m;
        AddOrderItemRequest request = new AddOrderItemRequest(orderId, menuItemId, addedQty);
        
        var existingOrderItem = new Api.Models.OrderItem
        {
            MenuItemId = menuItemId,
            Quantity = existingQty,
            Price = price
        };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem> { existingOrderItem },
            TotalAmount = price * existingQty
        };
        
        var menuItem = new Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Price = price
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedMenuItemRepo.Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync(menuItem);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        result.Should().BeOfType<Ok>();
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(expectedQty);
        order.TotalAmount.Should().Be(price * expectedQty);
    }

    [Fact]
    public async Task Given_MultipleItems_When_HandleIsCalled_Then_TotalCalculatedCorrectly()
    {
        int orderId = 1;
        int newMenuItemId = 20;
        int newItemQuantity = 2;
        decimal newItemPrice = 15.00m;
        AddOrderItemRequest request = new AddOrderItemRequest(orderId, newMenuItemId, newItemQuantity);
        
        var item1 = new Api.Models.OrderItem { MenuItemId = 10, Quantity = 3, Price = 10.00m };
        var item2 = new Api.Models.OrderItem { MenuItemId = 11, Quantity = 1, Price = 25.00m };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem> { item1, item2 },
            TotalAmount = 55.00m
        };
        
        var newMenuItem = new Api.Models.MenuItem { Id = newMenuItemId, Name = "New Item", Price = newItemPrice };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedMenuItemRepo.Setup(r => r.GetByIdAsync(newMenuItemId))
            .ReturnsAsync(newMenuItem);
        mockedOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        result.Should().BeOfType<Ok>();
        order.OrderItems.Should().HaveCount(3);
        order.TotalAmount.Should().Be(85.00m);
    }
}
