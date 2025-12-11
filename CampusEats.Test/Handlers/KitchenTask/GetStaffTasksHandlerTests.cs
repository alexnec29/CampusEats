using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class GetStaffTasksHandlerTests
{
    [Fact]
    public async Task Given_ValidStaffId_When_HandleIsCalled_Then_StaffTasksReturned()
    {
        // Arrange
        var staffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var tasks = new List<KitchenTaskResponse>
        {
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Task 1", AssignedTo = staffId.ToString() },
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Task 2", AssignedTo = staffId.ToString() }
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetStaffTasksAsync(staffId))
            .ReturnsAsync(tasks);
        
        var handler = new GetStaffTasksHandler(mockKitchenTaskRepository.Object);
        var request = new GetStaffTasksRequest(staffId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_StaffWithoutTasks_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        var staffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        mockKitchenTaskRepository.Setup(repo => repo.GetStaffTasksAsync(staffId))
            .ReturnsAsync(new List<KitchenTaskResponse>());
        
        var handler = new GetStaffTasksHandler(mockKitchenTaskRepository.Object);
        var request = new GetStaffTasksRequest(staffId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ValidStaffId_When_HandleIsCalled_Then_RepositoryCalledWithCorrectId()
    {
        // Arrange
        var staffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        mockKitchenTaskRepository.Setup(repo => repo.GetStaffTasksAsync(staffId))
            .ReturnsAsync(new List<KitchenTaskResponse>());
        
        var handler = new GetStaffTasksHandler(mockKitchenTaskRepository.Object);
        var request = new GetStaffTasksRequest(staffId);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(repo => repo.GetStaffTasksAsync(staffId), Times.Once);
    }
}
