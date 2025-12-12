using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.KitchenTask;

public class UpdateTaskStatusHandlerTests
{
    [Fact]
    public async Task Given_NonExistentTask_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentTaskId = 999;
        UpdateTaskStatusCommand command = new UpdateTaskStatusCommand(nonExistentTaskId, "Preparing");
        
        Mock<IKitchenTaskRepository> mockedTaskRepo = new Mock<IKitchenTaskRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedTaskRepo.Setup(r => r.GetByIdAsync(nonExistentTaskId))
            .ReturnsAsync((Api.Models.KitchenTask?)null);
        
        UpdateTaskStatusHandler handler = new UpdateTaskStatusHandler(
            mockedTaskRepo.Object,
            mockedOrderRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_InvalidStatus_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        int taskId = 1;
        UpdateTaskStatusCommand command = new UpdateTaskStatusCommand(taskId, "InvalidStatus");
        
        var task = new Api.Models.KitchenTask
        {
            Id = taskId,
            OrderId = 1,
            Status = OrderStatus.Pending
        };
        
        Mock<IKitchenTaskRepository> mockedTaskRepo = new Mock<IKitchenTaskRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedTaskRepo.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        UpdateTaskStatusHandler handler = new UpdateTaskStatusHandler(
            mockedTaskRepo.Object,
            mockedOrderRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_CompletedStatus_When_HandleIsCalled_Then_OrderStatusUpdatedToReady()
    {
        //Arrange
        int taskId = 1;
        int orderId = 10;
        UpdateTaskStatusCommand command = new UpdateTaskStatusCommand(taskId, "Completed");
        
        var task = new Api.Models.KitchenTask
        {
            Id = taskId,
            OrderId = orderId,
            Status = OrderStatus.Preparing
        };
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Preparing
        };
        
        Mock<IKitchenTaskRepository> mockedTaskRepo = new Mock<IKitchenTaskRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedTaskRepo.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        UpdateTaskStatusHandler handler = new UpdateTaskStatusHandler(
            mockedTaskRepo.Object,
            mockedOrderRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
        
        Assert.Equal(OrderStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
        
        mockedOrderRepo.Verify(r => r.UpdateAsync(It.Is<Api.Models.Order>(o =>
            o.Status == OrderStatus.Ready
        )), Times.Once);
    }
}
