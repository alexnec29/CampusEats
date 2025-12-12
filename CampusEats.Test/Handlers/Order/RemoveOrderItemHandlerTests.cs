using CampusEats.Api.Features.Order.RemoveOrderItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class RemoveOrderItemHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(nonExistentOrderId, 1);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        int orderId = 1;
        int orderItemId = 5;
        RemoveOrderItemRequest request = new RemoveOrderItemRequest(orderId, orderItemId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Placed // Cannot remove items from placed order
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        RemoveOrderItemHandler handler = new RemoveOrderItemHandler(mockedOrderRepo.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
}
