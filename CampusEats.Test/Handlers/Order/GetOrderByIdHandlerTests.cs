using CampusEats.Api.Features.Order;
using CampusEats.Api.Features.Order.GetOrderById;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class GetOrderByIdHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrderId_When_HandleIsCalled_Then_NotFoundReturned()
    {
        int nonExistentOrderId = 999;
        GetOrderByIdRequest request = new GetOrderByIdRequest(nonExistentOrderId);
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var notFoundResult = Assert.IsType<NotFound>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_ValidOrderId_When_HandleIsCalled_Then_OrderDetailResponseReturned()
    {
        int orderId = 1;
        Guid userId = Guid.NewGuid();
        GetOrderByIdRequest request = new GetOrderByIdRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100.50m,
            Status = OrderStatus.Placed,
            OrderDate = DateTime.UtcNow,
            Notes = "Test order",
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = 1, Quantity = 2, Price = 50.25m }
            },
            KitchenTask = new Api.Models.KitchenTask { Status = OrderStatus.Pending }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<OrderDetailResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().NotBeNull();
        okResult.Value.Id.Should().Be(orderId);
        okResult.Value.UserId.Should().Be(userId);
        okResult.Value.TotalAmount.Should().Be(100.50m);
        okResult.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_OrderWithoutKitchenTask_When_HandleIsCalled_Then_KitchenTaskIsNull()
    {
        int orderId = 1;
        GetOrderByIdRequest request = new GetOrderByIdRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Status = OrderStatus.Placed,
            OrderDate = DateTime.UtcNow,
            OrderItems = new List<Api.Models.OrderItem>(),
            KitchenTask = null
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<OrderDetailResponse>>(result);
        okResult.Value!.KitchenTask.Should().BeNull();
    }

    [Fact]
    public async Task Given_OrderWithKitchenTask_When_HandleIsCalled_Then_KitchenTaskIncluded()
    {
        int orderId = 1;
        Guid staffId = Guid.NewGuid();
        GetOrderByIdRequest request = new GetOrderByIdRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Status = OrderStatus.Preparing,
            OrderDate = DateTime.UtcNow,
            OrderItems = new List<Api.Models.OrderItem>(),
            KitchenTask = new Api.Models.KitchenTask 
            { 
                Status = OrderStatus.Preparing,
                AssignedStaffId = staffId
            }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<OrderDetailResponse>>(result);
        okResult.Value!.KitchenTask.Should().NotBeNull();
        okResult.Value.KitchenTask!.Status.Should().Be(OrderStatus.Preparing);
        okResult.Value.KitchenTask.AssignedStaffId.Should().Be(staffId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Given_OrderWithMultipleItems_When_HandleIsCalled_Then_AllItemsReturned(int itemCount)
    {
        int orderId = 1;
        GetOrderByIdRequest request = new GetOrderByIdRequest(orderId);
        
        var orderItems = new List<Api.Models.OrderItem>();
        for (int i = 0; i < itemCount; i++)
        {
            orderItems.Add(new Api.Models.OrderItem 
            { 
                MenuItemId = i + 1, 
                Quantity = i + 1, 
                Price = 10.00m * (i + 1) 
            });
        }
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Status = OrderStatus.Placed,
            OrderDate = DateTime.UtcNow,
            OrderItems = orderItems
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<OrderDetailResponse>>(result);
        okResult.Value!.Items.Should().HaveCount(itemCount);
    }

    [Fact]
    public async Task Given_OrderWithNotes_When_HandleIsCalled_Then_NotesIncluded()
    {
        int orderId = 1;
        string expectedNotes = "No onions please";
        GetOrderByIdRequest request = new GetOrderByIdRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Status = OrderStatus.Placed,
            OrderDate = DateTime.UtcNow,
            Notes = expectedNotes,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<OrderDetailResponse>>(result);
        okResult.Value!.Notes.Should().Be(expectedNotes);
    }
}
