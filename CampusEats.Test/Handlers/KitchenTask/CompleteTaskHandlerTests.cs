using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class CompleteTaskHandlerTests
{
    [Fact]
    public async Task Given_ValidTaskId_When_HandleIsCalled_Then_TaskIsCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CompleteTaskValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Title = "Task" };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new CompleteTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CompleteTaskRequest(taskId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentTask_When_HandleIsCalled_Then_NothingIsUpdated()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CompleteTaskValidator>();
        
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((Api.Models.KitchenTask)null);
        
        var handler = new CompleteTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CompleteTaskRequest(taskId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.KitchenTask>()), Times.Never);
    }
}
