using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.KitchenTask;

public class CreateKitchenTaskHandlerTests
{
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentOrderId = 999;
        CreateKitchenTaskCommand command = new CreateKitchenTaskCommand(nonExistentOrderId);
        
        Mock<IKitchenTaskRepository> mockedTaskRepository = new Mock<IKitchenTaskRepository>();
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        
        mockedOrderRepository.Setup(repo => repo.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        CreateKitchenTaskHandler handler = new CreateKitchenTaskHandler(
            mockedTaskRepository.Object,
            mockedOrderRepository.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("Order not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Given_DuplicateTaskForOrder_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        int orderId = 1;
        CreateKitchenTaskCommand command = new CreateKitchenTaskCommand(orderId);
        
        Mock<IKitchenTaskRepository> mockedTaskRepository = new Mock<IKitchenTaskRepository>();
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        
        mockedOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(new Api.Models.Order { Id = orderId });
        
        mockedTaskRepository.Setup(repo => repo.GetByOrderIdAsync(orderId))
            .ReturnsAsync(new Api.Models.KitchenTask { OrderId = orderId });
        
        CreateKitchenTaskHandler handler = new CreateKitchenTaskHandler(
            mockedTaskRepository.Object,
            mockedOrderRepository.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("already exists", badRequestResult.Value);
    }
    
    [Fact]
    public async Task Given_ValidOrder_When_HandleIsCalled_Then_TaskIsCreated()
    {
        //Arrange
        int orderId = 1;
        CreateKitchenTaskCommand command = new CreateKitchenTaskCommand(orderId);
        
        Mock<IKitchenTaskRepository> mockedTaskRepository = new Mock<IKitchenTaskRepository>();
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        
        mockedOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(new Api.Models.Order { Id = orderId });
        
        mockedTaskRepository.Setup(repo => repo.GetByOrderIdAsync(orderId))
            .ReturnsAsync((Api.Models.KitchenTask?)null);
        
        CreateKitchenTaskHandler handler = new CreateKitchenTaskHandler(
            mockedTaskRepository.Object,
            mockedOrderRepository.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<Api.Models.KitchenTask>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        Assert.Equal(orderId, okResult.Value.OrderId);
        Assert.Equal(OrderStatus.Pending, okResult.Value.Status);
        
        mockedTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.KitchenTask>()), Times.Once);
    }
}
