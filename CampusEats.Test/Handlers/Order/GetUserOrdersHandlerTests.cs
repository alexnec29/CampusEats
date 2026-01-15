using CampusEats.Api.Features.Order.GetUserOrders;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class GetUserOrdersHandlerTests
{
    [Fact]
    public async Task Given_UserWithOrders_When_HandleIsCalled_Then_OrdersListReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetUserOrdersRequest request = new GetUserOrdersRequest(userId);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = userId, Status = OrderStatus.Placed },
            new Api.Models.Order { Id = 2, UserId = userId, Status = OrderStatus.Completed }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        GetUserOrdersHandler handler = new GetUserOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().HaveCount(2);
        okResult.Value.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
    }
    
    [Fact]
    public async Task Given_UserWithNoOrders_When_HandleIsCalled_Then_EmptyListReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetUserOrdersRequest request = new GetUserOrdersRequest(userId);
        
        var emptyOrders = new List<Api.Models.Order>();
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(emptyOrders);
        
        GetUserOrdersHandler handler = new GetUserOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_UserWithMultipleStatusOrders_When_HandleIsCalled_Then_AllUserOrdersReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetUserOrdersRequest request = new GetUserOrdersRequest(userId);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = userId, Status = OrderStatus.Pending },
            new Api.Models.Order { Id = 2, UserId = userId, Status = OrderStatus.Placed },
            new Api.Models.Order { Id = 3, UserId = userId, Status = OrderStatus.Preparing },
            new Api.Models.Order { Id = 4, UserId = userId, Status = OrderStatus.Completed },
            new Api.Models.Order { Id = 5, UserId = userId, Status = OrderStatus.Cancelled }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        GetUserOrdersHandler handler = new GetUserOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(5);
        okResult.Value.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
        okResult.Value.Select(o => o.Status).Distinct().Should().HaveCount(5);
    }

    [Fact]
    public async Task Given_UserWithOrdersContainingItems_When_HandleIsCalled_Then_OrdersWithItemsReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetUserOrdersRequest request = new GetUserOrdersRequest(userId);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order 
            { 
                Id = 1, 
                UserId = userId, 
                Status = OrderStatus.Completed,
                TotalAmount = 45.00m,
                OrderItems = new List<Api.Models.OrderItem>
                {
                    new Api.Models.OrderItem { Id = 1, MenuItemId = 10, Quantity = 2, Price = 15.00m },
                    new Api.Models.OrderItem { Id = 2, MenuItemId = 11, Quantity = 1, Price = 15.00m }
                }
            },
            new Api.Models.Order 
            { 
                Id = 2, 
                UserId = userId, 
                Status = OrderStatus.Pending,
                TotalAmount = 20.00m,
                OrderItems = new List<Api.Models.OrderItem>
                {
                    new Api.Models.OrderItem { Id = 3, MenuItemId = 12, Quantity = 1, Price = 20.00m }
                }
            }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        GetUserOrdersHandler handler = new GetUserOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(2);
        okResult.Value[0].OrderItems.Should().HaveCount(2);
        okResult.Value[1].OrderItems.Should().HaveCount(1);
        okResult.Value.Sum(o => o.TotalAmount).Should().Be(65.00m);
    }

    [Fact]
    public async Task Given_DifferentUsers_When_HandleIsCalled_Then_OnlySpecificUserOrdersReturned()
    {
        //Arrange
        Guid targetUserId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        GetUserOrdersRequest request = new GetUserOrdersRequest(targetUserId);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = targetUserId, Status = OrderStatus.Placed },
            new Api.Models.Order { Id = 2, UserId = targetUserId, Status = OrderStatus.Completed }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByUserAsync(targetUserId))
            .ReturnsAsync(orders);
        
        GetUserOrdersHandler handler = new GetUserOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(2);
        okResult.Value.Should().AllSatisfy(o => o.UserId.Should().Be(targetUserId));
        okResult.Value.Should().AllSatisfy(o => o.UserId.Should().NotBe(otherUserId));
    }

    [Fact]
    public async Task Given_UserWithManyOrders_When_HandleIsCalled_Then_AllOrdersReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetUserOrdersRequest request = new GetUserOrdersRequest(userId);
        
        var orders = Enumerable.Range(1, 10)
            .Select(i => new Api.Models.Order 
            { 
                Id = i, 
                UserId = userId, 
                Status = OrderStatus.Completed,
                TotalAmount = i * 10.00m
            })
            .ToList();
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        GetUserOrdersHandler handler = new GetUserOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(10);
        okResult.Value.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
        okResult.Value.Sum(o => o.TotalAmount).Should().Be(550.00m);
    }
}
