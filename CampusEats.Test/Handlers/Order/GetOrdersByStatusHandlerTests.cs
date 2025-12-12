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
}
