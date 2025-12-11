using CampusEats.Api.Features.Order.UpdateOrderStatus;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Test.Handlers.Order;

public class UpdateOrderStatusHandlerTests
{
    [Fact]
    public async Task Given_ValidOrderAndStatus_When_HandleIsCalled_Then_StatusUpdated()
    {
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.Pending };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.Confirmed);

        var result = await handler.Handle(request, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Confirmed);
        mockOrderRepository.Verify(repo => repo.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_PendingToConfirmed_When_HandleIsCalled_Then_StatusChanges()
    {
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.Pending };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.Confirmed);

        await handler.Handle(request, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Given_ConfirmedToInProgress_When_HandleIsCalled_Then_StatusChanges()
    {
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.Confirmed };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.InProgress);

        await handler.Handle(request, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public async Task Given_InProgressToCompleted_When_HandleIsCalled_Then_StatusChanges()
    {
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.InProgress };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.Completed);

        await handler.Handle(request, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.Confirmed);

        var result = await handler.Handle(request, CancellationToken.None);

        var notFound = Assert.IsType<NotFound>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Given_ValidTransition_When_HandleIsCalled_Then_RepositoryCalledOnce()
    {
        var orderId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Api.Models.Order { Id = orderId, Status = OrderStatus.Pending };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.Confirmed);

        await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }
}

public class GetOrdersByStatusHandlerTests
{
    [Fact]
    public async Task Given_PendingOrders_When_HandleIsCalled_Then_PendingOrdersReturned()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending },
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending }
        };
        
        mockOrderRepository.Setup(repo => repo.GetOrdersByStatusAsync(OrderStatus.Pending))
            .ReturnsAsync(orders);
        
        var handler = new GetOrdersByStatusHandler(mockOrderRepository.Object);
        var request = new GetOrdersByStatusRequest(OrderStatus.Pending);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(2);
        result.All(o => o.Status == OrderStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task Given_CompletedOrders_When_HandleIsCalled_Then_CompletedOrdersReturned()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Completed }
        };
        
        mockOrderRepository.Setup(repo => repo.GetOrdersByStatusAsync(OrderStatus.Completed))
            .ReturnsAsync(orders);
        
        var handler = new GetOrdersByStatusHandler(mockOrderRepository.Object);
        var request = new GetOrdersByStatusRequest(OrderStatus.Completed);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Given_NoOrdersWithStatus_When_HandleIsCalled_Then_EmptyListReturned()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        mockOrderRepository.Setup(repo => repo.GetOrdersByStatusAsync(OrderStatus.Cancelled))
            .ReturnsAsync(new List<Api.Models.Order>());
        
        var handler = new GetOrdersByStatusHandler(mockOrderRepository.Object);
        var request = new GetOrdersByStatusRequest(OrderStatus.Cancelled);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_DifferentStatuses_When_HandleIsCalled_Then_CorrectStatusesReturned()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var pendingOrders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending }
        };
        var confirmedOrders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Confirmed }
        };
        
        mockOrderRepository.Setup(repo => repo.GetOrdersByStatusAsync(OrderStatus.Pending))
            .ReturnsAsync(pendingOrders);
        mockOrderRepository.Setup(repo => repo.GetOrdersByStatusAsync(OrderStatus.Confirmed))
            .ReturnsAsync(confirmedOrders);
        
        var handler = new GetOrdersByStatusHandler(mockOrderRepository.Object);
        var requestPending = new GetOrdersByStatusRequest(OrderStatus.Pending);
        var requestConfirmed = new GetOrdersByStatusRequest(OrderStatus.Confirmed);

        var resultPending = await handler.Handle(requestPending, CancellationToken.None);
        var resultConfirmed = await handler.Handle(requestConfirmed, CancellationToken.None);

        resultPending.All(o => o.Status == OrderStatus.Pending).Should().BeTrue();
        resultConfirmed.All(o => o.Status == OrderStatus.Confirmed).Should().BeTrue();
    }

    [Fact]
    public async Task Given_MultipleOrdersSameStatus_When_HandleIsCalled_Then_AllReturned()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.InProgress, TotalAmount = 50m },
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.InProgress, TotalAmount = 75m },
            new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.InProgress, TotalAmount = 100m }
        };
        
        mockOrderRepository.Setup(repo => repo.GetOrdersByStatusAsync(OrderStatus.InProgress))
            .ReturnsAsync(orders);
        
        var handler = new GetOrdersByStatusHandler(mockOrderRepository.Object);
        var request = new GetOrdersByStatusRequest(OrderStatus.InProgress);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(o => o.TotalAmount).Should().Contain(new[] { 50m, 75m, 100m });
    }
}

public class AddOrderItemHandlerTests
{
    [Fact]
    public async Task Given_ValidOrderAndItem_When_HandleIsCalled_Then_ItemAddedToOrder()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new AddOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new AddOrderItemRequest(orderId, menuItemId, 2);

        var result = await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NothingIsAdded()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new AddOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new AddOrderItemRequest(orderId, menuItemId, 1);

        var result = await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Never);
    }

    [Fact]
    public async Task Given_MultipleItems_When_HandleIsCalled_Then_AllItemsAdded()
    {
        var orderId = Guid.NewGuid();
        var menuItemId1 = Guid.NewGuid();
        var menuItemId2 = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new AddOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request1 = new AddOrderItemRequest(orderId, menuItemId1, 1);
        var request2 = new AddOrderItemRequest(orderId, menuItemId2, 2);

        await handler.Handle(request1, CancellationToken.None);
        await handler.Handle(request2, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Given_PositiveQuantity_When_HandleIsCalled_Then_QuantityStored()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new AddOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new AddOrderItemRequest(orderId, menuItemId, 5);

        await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }
}

public class RemoveOrderItemHandlerTests
{
    [Fact]
    public async Task Given_ValidOrderAndItem_When_HandleIsCalled_Then_ItemRemovedFromOrder()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<RemoveOrderItemValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        var orderItem = new Api.Models.OrderItem { Id = orderItemId, OrderId = orderId };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(repo => repo.GetOrderItemAsync(orderId, orderItemId))
            .ReturnsAsync(orderItem);
        
        var handler = new RemoveOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new RemoveOrderItemRequest(orderId, orderItemId);

        var result = await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.RemoveOrderItemAsync(orderItem), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NothingIsRemoved()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<RemoveOrderItemValidator>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new RemoveOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new RemoveOrderItemRequest(orderId, orderItemId);

        var result = await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.RemoveOrderItemAsync(It.IsAny<Api.Models.OrderItem>()), Times.Never);
    }

    [Fact]
    public async Task Given_NonExistentOrderItem_When_HandleIsCalled_Then_NothingIsRemoved()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<RemoveOrderItemValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(repo => repo.GetOrderItemAsync(orderId, orderItemId))
            .ReturnsAsync((Api.Models.OrderItem)null);
        
        var handler = new RemoveOrderItemHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new RemoveOrderItemRequest(orderId, orderItemId);

        var result = await handler.Handle(request, CancellationToken.None);

        mockOrderRepository.Verify(repo => repo.RemoveOrderItemAsync(It.IsAny<Api.Models.OrderItem>()), Times.Never);
    }
}

public class UpdateOrderItemQuantityHandlerTests
{
    [Fact]
    public async Task Given_ValidOrderAndQuantity_When_HandleIsCalled_Then_QuantityUpdated()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderItemQuantityValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        var orderItem = new Api.Models.OrderItem { Id = orderItemId, Quantity = 2 };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(repo => repo.GetOrderItemAsync(orderId, orderItemId))
            .ReturnsAsync(orderItem);
        
        var handler = new UpdateOrderItemQuantityHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderItemQuantityRequest(orderId, orderItemId, 5);

        var result = await handler.Handle(request, CancellationToken.None);

        orderItem.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Given_IncreaseQuantity_When_HandleIsCalled_Then_QuantityIncreased()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderItemQuantityValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        var orderItem = new Api.Models.OrderItem { Id = orderItemId, Quantity = 2 };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(repo => repo.GetOrderItemAsync(orderId, orderItemId))
            .ReturnsAsync(orderItem);
        
        var handler = new UpdateOrderItemQuantityHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderItemQuantityRequest(orderId, orderItemId, 10);

        await handler.Handle(request, CancellationToken.None);

        orderItem.Quantity.Should().Be(10);
    }

    [Fact]
    public async Task Given_DecreaseQuantity_When_HandleIsCalled_Then_QuantityDecreased()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderItemQuantityValidator>();
        
        var order = new Api.Models.Order { Id = orderId };
        var orderItem = new Api.Models.OrderItem { Id = orderItemId, Quantity = 10 };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(repo => repo.GetOrderItemAsync(orderId, orderItemId))
            .ReturnsAsync(orderItem);
        
        var handler = new UpdateOrderItemQuantityHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new UpdateOrderItemQuantityRequest(orderId, orderItemId, 2);

        await handler.Handle(request, CancellationToken.None);

        orderItem.Quantity.Should().Be(2);
    }
}
