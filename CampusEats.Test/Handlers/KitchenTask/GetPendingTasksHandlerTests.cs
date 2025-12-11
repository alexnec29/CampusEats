using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class GetPendingTasksHandlerTests
{
    [Fact]
    public async Task Given_PendingTasksExist_When_HandleIsCalled_Then_PendingTasksReturned()
    {
        // Arrange
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var tasks = new List<KitchenTaskResponse>
        {
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Task 1", Status = "Pending" },
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Task 2", Status = "Pending" }
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetPendingTasksAsync())
            .ReturnsAsync(tasks);
        
        var handler = new GetPendingTasksHandler(mockKitchenTaskRepository.Object);
        var request = new GetPendingTasksRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_NoPendingTasks_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        mockKitchenTaskRepository.Setup(repo => repo.GetPendingTasksAsync())
            .ReturnsAsync(new List<KitchenTaskResponse>());
        
        var handler = new GetPendingTasksHandler(mockKitchenTaskRepository.Object);
        var request = new GetPendingTasksRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
