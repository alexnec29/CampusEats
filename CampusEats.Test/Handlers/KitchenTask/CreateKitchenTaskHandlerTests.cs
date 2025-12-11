using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class CreateKitchenTaskHandlerTests
{
    [Fact]
    public async Task Given_ValidTaskRequest_When_HandleIsCalled_Then_TaskIsCreated()
    {
        // Arrange
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Prepare Order #123", "Prepare burger and fries", null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.KitchenTask>()), Times.Once);
    }

    [Fact]
    public async Task Given_TaskWithAssignee_When_HandleIsCalled_Then_TaskIsCreatedWithAssignee()
    {
        // Arrange
        var assigneeId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Prep Task", "Description", assigneeId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.KitchenTask>(
                t => t.AssignedToUserId == assigneeId
            )), Times.Once);
    }

    [Fact]
    public async Task Given_TaskWithoutAssignee_When_HandleIsCalled_Then_TaskIsCreatedUnassigned()
    {
        // Arrange
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Unassigned Task", "Description", null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.KitchenTask>(
                t => t.AssignedToUserId == null
            )), Times.Once);
    }
}
