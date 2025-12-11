using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class UpdateTaskStatusHandlerTests
{
    [Fact]
    public async Task Given_ValidTaskAndStatus_When_HandleIsCalled_Then_StatusIsUpdated()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<UpdateTaskStatusValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Status = OrderStatus.Pending };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new UpdateTaskStatusHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new UpdateTaskStatusRequest(taskId, OrderStatus.InProgress);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        task.Status.Should().Be(OrderStatus.InProgress);
        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentTask_When_HandleIsCalled_Then_RepositoryNotCalled()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<UpdateTaskStatusValidator>();
        
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((Api.Models.KitchenTask)null);
        
        var handler = new UpdateTaskStatusHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new UpdateTaskStatusRequest(taskId, OrderStatus.Completed);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.KitchenTask>()), Times.Never);
    }

    [Fact]
    public async Task Given_TaskToComplete_When_HandleIsCalled_Then_StatusIsCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<UpdateTaskStatusValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Status = OrderStatus.InProgress };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new UpdateTaskStatusHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new UpdateTaskStatusRequest(taskId, OrderStatus.Completed);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        task.Status.Should().Be(OrderStatus.Completed);
    }
}
