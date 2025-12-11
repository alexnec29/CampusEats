using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class GetTaskByIdHandlerTests
{
    [Fact]
    public async Task Given_ValidTaskId_When_HandleIsCalled_Then_TaskIsReturned()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var task = new Api.Models.KitchenTask { Id = taskId, Title = "Test Task" };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new GetTaskByIdHandler(mockKitchenTaskRepository.Object);
        var request = new GetTaskByIdRequest(taskId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task Given_NonExistentTaskId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((Api.Models.KitchenTask)null);
        
        var handler = new GetTaskByIdHandler(mockKitchenTaskRepository.Object);
        var request = new GetTaskByIdRequest(taskId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
