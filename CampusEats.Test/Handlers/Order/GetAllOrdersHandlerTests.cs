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
        //Arrange
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
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task Given_NoOrders_When_HandleIsCalled_Then_EmptyListReturned()
    {
        //Arrange
        GetAllOrdersRequest request = new GetAllOrdersRequest();
        
        var emptyOrders = new List<Api.Models.Order>();
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(emptyOrders);
        
        GetAllOrdersHandler handler = new GetAllOrdersHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<List<OrderResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().BeEmpty();
    }
}
