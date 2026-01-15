using CampusEats.Api.Features.Order.UpdateOrderStatus;
using CampusEats.Api.Features.Loyalty.EarnPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class UpdateOrderStatusHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        int nonExistentOrderId = 999;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(nonExistentOrderId, OrderStatus.Placed);
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var notFoundResult = Assert.IsType<NotFound>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_InvalidStatusTransition_When_HandleIsCalled_Then_BadRequestReturned()
    {
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Completed);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = OrderStatus.Pending
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("Invalid status transition", badRequestResult.Value);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Placed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Placed, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Placed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Placed, OrderStatus.Paid)]
    [InlineData(OrderStatus.Paid, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Ready)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Ready, OrderStatus.Completed)]
    public async Task Given_ValidStatusTransition_When_HandleIsCalled_Then_OrderUpdated(OrderStatus fromStatus, OrderStatus toStatus)
    {
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, toStatus);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = fromStatus,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.0m
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok>(result);
        order.Status.Should().Be(toStatus);
        mockedRepository.Verify(repo => repo.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_CompletedStatus_When_HandleIsCalled_Then_LoyaltyPointsEarned()
    {
        int orderId = 1;
        Guid userId = Guid.NewGuid();
        decimal totalAmount = 100.0m;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Completed);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = OrderStatus.Ready,
            UserId = userId,
            TotalAmount = totalAmount
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        mockedMediator.Setup(m => m.Send(It.IsAny<EarnPointsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Results.Ok());
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok>(result);
        mockedMediator.Verify(m => m.Send(
            It.Is<EarnPointsRequest>(r => r.UserId == userId && r.OrderId == orderId && r.OrderAmount == totalAmount), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Given_CompletedStatusWithZeroAmount_When_HandleIsCalled_Then_NoLoyaltyPoints()
    {
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Completed);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = OrderStatus.Ready,
            UserId = Guid.NewGuid(),
            TotalAmount = 0
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok>(result);
        mockedMediator.Verify(m => m.Send(It.IsAny<EarnPointsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Given_LoyaltyPointsFailure_When_HandleIsCalled_Then_OrderStillUpdated()
    {
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Completed);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = OrderStatus.Ready,
            UserId = Guid.NewGuid(),
            TotalAmount = 100.0m
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        mockedMediator.Setup(m => m.Send(It.IsAny<EarnPointsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Loyalty service failed"));
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok>(result);
        order.Status.Should().Be(OrderStatus.Completed);
        mockedRepository.Verify(repo => repo.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Given_OrderWithKitchenTask_When_HandleIsCalled_Then_KitchenTaskStatusUpdated()
    {
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Preparing);
        
        var kitchenTask = new Api.Models.KitchenTask 
        { 
            Id = 1, 
            Status = OrderStatus.Placed 
        };
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = OrderStatus.Placed,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.0m,
            KitchenTask = kitchenTask
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        mockedRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var okResult = Assert.IsType<Ok>(result);
        order.KitchenTask.Status.Should().Be(OrderStatus.Preparing);
    }

    [Theory]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Given_CompletedOrCancelledStatus_When_HandleIsCalled_Then_BadRequest(OrderStatus invalidFromStatus)
    {
        int orderId = 1;
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(orderId, OrderStatus.Preparing);
        
        var order = new Api.Models.Order 
        { 
            Id = orderId, 
            Status = invalidFromStatus
        };
        
        Mock<IOrderRepository> mockedRepository = new Mock<IOrderRepository>();
        Mock<IMediator> mockedMediator = new Mock<IMediator>();
        Mock<ILogger<UpdateOrderStatusHandler>> mockedLogger = new Mock<ILogger<UpdateOrderStatusHandler>>();
        UpdateOrderStatusValidator validator = new UpdateOrderStatusValidator();
        
        mockedRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        UpdateOrderStatusHandler handler = new UpdateOrderStatusHandler(
            mockedRepository.Object,
            validator,
            mockedMediator.Object,
            mockedLogger.Object
        );
        
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        var badRequestResult = result.Should().BeOfType<BadRequest<string>>().Subject;
        badRequestResult.Value.Should().Contain("Cannot update status from");
    }
}
