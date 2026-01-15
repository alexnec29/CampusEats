using CampusEats.Api.Features.Order.GetOrdersByStatus;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class GetOrdersByStatusHandlerTests
{
    [Fact]
    public async Task Given_StatusWithOrders_When_HandleIsCalled_Then_OrdersListReturned()
    {
        //Arrange
        OrderStatus status = OrderStatus.Pending;
        GetOrdersByStatusRequest request = new GetOrdersByStatusRequest(status);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = Guid.NewGuid(), Status = OrderStatus.Pending },
            new Api.Models.Order { Id = 2, UserId = Guid.NewGuid(), Status = OrderStatus.Pending }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByStatusAsync(status))
            .ReturnsAsync(orders);
        
        GetOrdersByStatusHandler handler = new GetOrdersByStatusHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().HaveCount(2);
        okResult.Value.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }
    
    [Fact]
    public async Task Given_StatusWithNoOrders_When_HandleIsCalled_Then_EmptyListReturned()
    {
        //Arrange
        OrderStatus status = OrderStatus.Cancelled;
        GetOrdersByStatusRequest request = new GetOrdersByStatusRequest(status);
        
        var emptyOrders = new List<Api.Models.Order>();
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByStatusAsync(status))
            .ReturnsAsync(emptyOrders);
        
        GetOrdersByStatusHandler handler = new GetOrdersByStatusHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Given_DifferentStatuses_When_HandleIsCalled_Then_OnlyMatchingStatusReturned(OrderStatus requestedStatus)
    {
        //Arrange
        GetOrdersByStatusRequest request = new GetOrdersByStatusRequest(requestedStatus);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = Guid.NewGuid(), Status = requestedStatus },
            new Api.Models.Order { Id = 2, UserId = Guid.NewGuid(), Status = requestedStatus },
            new Api.Models.Order { Id = 3, UserId = Guid.NewGuid(), Status = requestedStatus }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByStatusAsync(requestedStatus))
            .ReturnsAsync(orders);
        
        GetOrdersByStatusHandler handler = new GetOrdersByStatusHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(3);
        okResult.Value.Should().AllSatisfy(o => o.Status.Should().Be(requestedStatus));
    }

    [Fact]
    public async Task Given_MultipleUsersWithSameStatus_When_HandleIsCalled_Then_AllOrdersReturned()
    {
        //Arrange
        OrderStatus status = OrderStatus.Preparing;
        GetOrdersByStatusRequest request = new GetOrdersByStatusRequest(status);
        
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = user1, Status = OrderStatus.Preparing },
            new Api.Models.Order { Id = 2, UserId = user2, Status = OrderStatus.Preparing },
            new Api.Models.Order { Id = 3, UserId = user3, Status = OrderStatus.Preparing },
            new Api.Models.Order { Id = 4, UserId = user1, Status = OrderStatus.Preparing }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByStatusAsync(status))
            .ReturnsAsync(orders);
        
        GetOrdersByStatusHandler handler = new GetOrdersByStatusHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(4);
        okResult.Value.Select(o => o.UserId).Distinct().Should().HaveCount(3);
    }

    [Fact]
    public async Task Given_CompletedOrders_When_HandleIsCalled_Then_OnlyCompletedReturned()
    {
        //Arrange
        OrderStatus status = OrderStatus.Completed;
        GetOrdersByStatusRequest request = new GetOrdersByStatusRequest(status);
        
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = Guid.NewGuid(), Status = OrderStatus.Completed, TotalAmount = 25.00m },
            new Api.Models.Order { Id = 2, UserId = Guid.NewGuid(), Status = OrderStatus.Completed, TotalAmount = 50.00m },
            new Api.Models.Order { Id = 3, UserId = Guid.NewGuid(), Status = OrderStatus.Completed, TotalAmount = 15.00m }
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetOrdersByStatusAsync(status))
            .ReturnsAsync(orders);
        
        GetOrdersByStatusHandler handler = new GetOrdersByStatusHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.Order>>>(result);
        okResult.Value.Should().HaveCount(3);
        okResult.Value.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Completed));
        okResult.Value.Sum(o => o.TotalAmount).Should().Be(90.00m);
    }
}
