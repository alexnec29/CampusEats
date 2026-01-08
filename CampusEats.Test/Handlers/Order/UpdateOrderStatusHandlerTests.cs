using CampusEats.Api.Features.Order.UpdateOrderStatus;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class UpdateOrderStatusHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(nonExistentOrderId, OrderStatus.Placed);
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_InvalidStatusTransition_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Completed);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = OrderStatus.Pending // Pending -> Completed is not allowed
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("Invalid status transition", badRequestResult.Value);
    }
}
