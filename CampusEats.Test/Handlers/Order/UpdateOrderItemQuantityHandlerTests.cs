using CampusEats.Api.Features.Order.UpdateOrderItemQuantity;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class UpdateOrderItemQuantityHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(nonExistentOrderId, 1, 5);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonExistentOrderItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int orderId = 1;
        int nonExistentItemId = 999;
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(orderId, nonExistentItemId, 5);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        UpdateOrderItemQuantityValidator validator = new UpdateOrderItemQuantityValidator();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        UpdateOrderItemQuantityHandler handler = new UpdateOrderItemQuantityHandler(
            mockedOrderRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
}
