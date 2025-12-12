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
}
