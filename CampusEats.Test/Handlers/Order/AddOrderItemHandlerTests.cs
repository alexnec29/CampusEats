using CampusEats.Api.Features.Order.AddOrderItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class AddOrderItemHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        AddOrderItemRequest request = new AddOrderItemRequest(nonExistentOrderId, 1, 2);
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
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
    public async Task Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        int orderId = 1;
        AddOrderItemRequest request = new AddOrderItemRequest(orderId, 1, 2);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Completed // Cannot add items to completed order
        };
        
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        AddOrderItemValidator validator = new AddOrderItemValidator(mockedMenuItemRepo.Object);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        AddOrderItemHandler handler = new AddOrderItemHandler(
            mockedOrderRepo.Object,
            mockedMenuItemRepo.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
}
