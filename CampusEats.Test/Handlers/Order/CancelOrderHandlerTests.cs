using CampusEats.Api.Features.Order.CancelOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class CancelOrderHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        CancelOrderRequest request = new CancelOrderRequest(nonExistentOrderId);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
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
        CancelOrderRequest request = new CancelOrderRequest(orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Completed // Cannot cancel completed order
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        CancelOrderValidator validator = new CancelOrderValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        CancelOrderHandler handler = new CancelOrderHandler(mockedOrderRepo.Object, validator);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
}
