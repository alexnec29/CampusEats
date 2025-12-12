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
        //Arrange
        int nonExistentOrderId = 999;
        GetOrderByIdRequest request = new GetOrderByIdRequest(nonExistentOrderId);
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        mockedRepository.Setup(repo => repo.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        GetOrderByIdHandler handler = new GetOrderByIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_ValidOrderId_When_HandleIsCalled_Then_OrderDetailResponseReturned()
    {
        //Arrange
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
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<OrderDetailResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().NotBeNull();
        okResult.Value.Id.Should().Be(orderId);
        okResult.Value.UserId.Should().Be(userId);
        okResult.Value.TotalAmount.Should().Be(100.50m);
        okResult.Value.Items.Should().HaveCount(1);
    }
}
