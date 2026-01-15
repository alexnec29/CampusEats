using CampusEats.Api.Features.Order;
using CampusEats.Api.Features.Order.GetAllOrders;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class GetAllOrdersHandlerTests
{
    [Fact]
    public async Task Given_OrdersExist_When_HandleIsCalled_Then_AllOrdersReturned()
    {
        GetAllOrdersRequest request = new GetAllOrdersRequest();
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order 
            { 
                Id = 1, 
                UserId = Guid.NewGuid(), 
                Status = OrderStatus.Pending,
                TotalAmount = 25.50m,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<Api.Models.OrderItem>()
            },
            new Api.Models.Order 
            { 
                Id = 2, 
                UserId = Guid.NewGuid(), 
                Status = OrderStatus.Completed,
                TotalAmount = 40.00m,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<Api.Models.OrderItem>()
            }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        GetAllOrdersHandler handler = new GetAllOrdersHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task Given_NoOrders_When_HandleIsCalled_Then_EmptyListReturned()
    {
        GetAllOrdersRequest request = new GetAllOrdersRequest();
        
        var emptyOrders = new List<Api.Models.Order>();
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(emptyOrders);
        
        GetAllOrdersHandler handler = new GetAllOrdersHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_OrdersWithItems_When_HandleIsCalled_Then_OrderItemsIncluded()
    {
        GetAllOrdersRequest request = new GetAllOrdersRequest();
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order 
            { 
                Id = 1, 
                UserId = Guid.NewGuid(), 
                Status = OrderStatus.Placed,
                TotalAmount = 45.00m,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<Api.Models.OrderItem>
                {
                    new Api.Models.OrderItem { MenuItemId = 10, Quantity = 2, Price = 15.00m },
                    new Api.Models.OrderItem { MenuItemId = 11, Quantity = 1, Price = 15.00m }
                }
            }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        GetAllOrdersHandler handler = new GetAllOrdersHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        var orderResponse = okResult.Value!.First();
        orderResponse.Items.Should().HaveCount(2);
        orderResponse.Items.First().MenuItemId.Should().Be(10);
        orderResponse.Items.First().Quantity.Should().Be(2);
        orderResponse.Items.First().Price.Should().Be(15.00m);
    }

    [Theory]
    [InlineData(OrderStatus.Inactive)]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Given_OrdersWithDifferentStatuses_When_HandleIsCalled_Then_AllStatusesReturned(OrderStatus status)
    {
        GetAllOrdersRequest request = new GetAllOrdersRequest();
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order 
            { 
                Id = 1, 
                UserId = Guid.NewGuid(), 
                Status = status,
                TotalAmount = 25.50m,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<Api.Models.OrderItem>()
            }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        GetAllOrdersHandler handler = new GetAllOrdersHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        okResult.Value!.First().Status.Should().Be(status);
    }

    [Fact]
    public async Task Given_OrdersWithNotes_When_HandleIsCalled_Then_NotesIncluded()
    {
        GetAllOrdersRequest request = new GetAllOrdersRequest();
        string expectedNotes = "Extra spicy please";
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order 
            { 
                Id = 1, 
                UserId = Guid.NewGuid(), 
                Status = OrderStatus.Placed,
                TotalAmount = 25.50m,
                OrderDate = DateTime.UtcNow,
                Notes = expectedNotes,
                OrderItems = new List<Api.Models.OrderItem>()
            }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        GetAllOrdersHandler handler = new GetAllOrdersHandler(mockedRepository.Object);
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        okResult.Value!.First().Notes.Should().Be(expectedNotes);
    }
}
