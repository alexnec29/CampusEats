using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class AssignTaskToStaffHandlerTests
{
    [Fact]
    public async Task Given_ValidTaskAndStaff_When_HandleIsCalled_Then_TaskIsAssigned()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<AssignTaskToStaffValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Title = "Task" };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new AssignTaskToStaffHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new AssignTaskToStaffRequest(taskId, staffId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        task.AssignedToUserId.Should().Be(staffId);
        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentTask_When_HandleIsCalled_Then_NothingIsUpdated()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<AssignTaskToStaffValidator>();
        
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((Api.Models.KitchenTask)null);
        
        var handler = new AssignTaskToStaffHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new AssignTaskToStaffRequest(taskId, staffId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.KitchenTask>()), Times.Never);
    }

    [Fact]
    public async Task Given_TaskWithExistingAssignment_When_HandleIsCalled_Then_AssignmentIsUpdated()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var oldStaffId = Guid.NewGuid();
        var newStaffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<AssignTaskToStaffValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, AssignedToUserId = oldStaffId };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new AssignTaskToStaffHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new AssignTaskToStaffRequest(taskId, newStaffId);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        task.AssignedToUserId.Should().Be(newStaffId);
    }
}
