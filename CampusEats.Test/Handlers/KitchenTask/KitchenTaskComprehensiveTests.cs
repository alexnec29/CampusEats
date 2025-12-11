using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class KitchenTaskComprehensiveTests
{
    [Fact]
    public async Task Given_CreateTaskWithTitle_When_HandleCalled_Then_TitleStored()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Prep burgers", "Prepare 10 burgers", null);

        await handler.Handle(request, CancellationToken.None);

        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.KitchenTask>(t => t.Title == "Prep burgers")),
            Times.Once);
    }

    [Fact]
    public async Task Given_CreateTaskWithDescription_When_HandleCalled_Then_DescriptionStored()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Task", "Detailed description here", null);

        await handler.Handle(request, CancellationToken.None);

        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.KitchenTask>(t => t.Description == "Detailed description here")),
            Times.Once);
    }

    [Fact]
    public async Task Given_CreateTaskWithAssignee_When_HandleCalled_Then_AssigneeStored()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        var assigneeId = Guid.NewGuid();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Task", "Desc", assigneeId);

        await handler.Handle(request, CancellationToken.None);

        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.KitchenTask>(t => t.AssignedToUserId == assigneeId)),
            Times.Once);
    }

    [Fact]
    public async Task Given_CreateMultipleTasksSequentially_When_HandleCalled_Then_AllCreated()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 5; i++)
        {
            var request = new CreateKitchenTaskRequest($"Task {i}", "Description", null);
            await handler.Handle(request, CancellationToken.None);
        }

        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.IsAny<Api.Models.KitchenTask>()),
            Times.Exactly(5));
    }

    [Fact]
    public async Task Given_TaskStatusPending_When_UpdateToInProgress_Then_StatusChanged()
    {
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<UpdateTaskStatusValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Status = OrderStatus.Pending };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new UpdateTaskStatusHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new UpdateTaskStatusRequest(taskId, OrderStatus.InProgress);

        await handler.Handle(request, CancellationToken.None);

        task.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public async Task Given_TaskStatusInProgress_When_UpdateToCompleted_Then_StatusChanged()
    {
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<UpdateTaskStatusValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Status = OrderStatus.InProgress };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new UpdateTaskStatusHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new UpdateTaskStatusRequest(taskId, OrderStatus.Completed);

        await handler.Handle(request, CancellationToken.None);

        task.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Given_TaskWithMultipleStatusChanges_When_Updated_Then_LatestStatusKept()
    {
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<UpdateTaskStatusValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, Status = OrderStatus.Pending };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new UpdateTaskStatusHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        
        await handler.Handle(new UpdateTaskStatusRequest(taskId, OrderStatus.InProgress), CancellationToken.None);
        task.Status.Should().Be(OrderStatus.InProgress);
        
        await handler.Handle(new UpdateTaskStatusRequest(taskId, OrderStatus.Completed), CancellationToken.None);
        task.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Given_GetTaskWithValidId_When_HandleCalled_Then_TaskReturned()
    {
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var task = new Api.Models.KitchenTask { Id = taskId, Title = "Test Task" };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new GetTaskByIdHandler(mockKitchenTaskRepository.Object);
        var request = new GetTaskByIdRequest(taskId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task Given_AssignTaskToStaff_When_HandleCalled_Then_StaffAssigned()
    {
        var taskId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<AssignTaskToStaffValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new AssignTaskToStaffHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new AssignTaskToStaffRequest(taskId, staffId);

        await handler.Handle(request, CancellationToken.None);

        task.AssignedToUserId.Should().Be(staffId);
    }

    [Fact]
    public async Task Given_ReassignTaskToAnotherStaff_When_HandleCalled_Then_StaffChanged()
    {
        var taskId = Guid.NewGuid();
        var staff1 = Guid.NewGuid();
        var staff2 = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<AssignTaskToStaffValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId, AssignedToUserId = staff1 };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new AssignTaskToStaffHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new AssignTaskToStaffRequest(taskId, staff2);

        await handler.Handle(request, CancellationToken.None);

        task.AssignedToUserId.Should().Be(staff2);
    }

    [Fact]
    public async Task Given_CompleteTask_When_HandleCalled_Then_TaskCompleted()
    {
        var taskId = Guid.NewGuid();
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CompleteTaskValidator>();
        
        var task = new Api.Models.KitchenTask { Id = taskId };
        mockKitchenTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        var handler = new CompleteTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CompleteTaskRequest(taskId);

        await handler.Handle(request, CancellationToken.None);

        mockKitchenTaskRepository.Verify(repo => repo.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task Given_GetPendingTasks_When_HandleCalled_Then_PendingTasksReturned()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var tasks = new List<KitchenTaskResponse>
        {
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Pending 1", Status = "Pending" },
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Pending 2", Status = "Pending" },
            new KitchenTaskResponse { Id = Guid.NewGuid(), Title = "Pending 3", Status = "Pending" }
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetPendingTasksAsync())
            .ReturnsAsync(tasks);
        
        var handler = new GetPendingTasksHandler(mockKitchenTaskRepository.Object);
        var request = new GetPendingTasksRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(3);
        result.All(t => t.Status == "Pending").Should().BeTrue();
    }

    [Fact]
    public async Task Given_GetStaffTasks_When_HandleCalled_Then_StaffTasksReturned()
    {
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

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(2);
        result.All(t => t.AssignedTo == staffId.ToString()).Should().BeTrue();
    }

    [Fact]
    public async Task Given_GetDailyReport_When_HandleCalled_Then_ReportReturned()
    {
        var today = DateTime.Now.Date;
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var report = new KitchenDailyReportResponse
        {
            Date = today,
            TotalTasks = 15,
            CompletedTasks = 10,
            PendingTasks = 5
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetDailyReportAsync(today))
            .ReturnsAsync(report);
        
        var handler = new GetDailyReportHandler(mockKitchenTaskRepository.Object);
        var request = new GetDailyReportRequest(today);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalTasks.Should().Be(15);
        result.CompletedTasks.Should().Be(10);
        result.PendingTasks.Should().Be(5);
    }
}

public class KitchenTaskEdgeCaseTests
{
    [Fact]
    public async Task Given_CreateTaskWithVeryLongTitle_When_HandleCalled_Then_Stored()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        var longTitle = new string('A', 500);
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest(longTitle, "Description", null);

        await handler.Handle(request, CancellationToken.None);

        mockKitchenTaskRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.KitchenTask>(t => t.Title == longTitle)),
            Times.Once);
    }

    [Fact]
    public async Task Given_CreateTaskWithSpecialCharactersInTitle_When_HandleCalled_Then_Stored()
    {
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var handler = new CreateKitchenTaskHandler(mockKitchenTaskRepository.Object, mockValidator.Object);
        var request = new CreateKitchenTaskRequest("Task #1: Prep & Cook (Special)", "Desc", null);

        await handler.Handle(request, CancellationToken.None);

        mockKitchenTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.KitchenTask>()), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleTasksForDailyReport_When_HandleCalled_Then_AllCounted()
    {
        var today = DateTime.Now.Date;
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var report = new KitchenDailyReportResponse
        {
            Date = today,
            TotalTasks = 50,
            CompletedTasks = 30,
            PendingTasks = 20
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetDailyReportAsync(today))
            .ReturnsAsync(report);
        
        var handler = new GetDailyReportHandler(mockKitchenTaskRepository.Object);
        var request = new GetDailyReportRequest(today);

        var result = await handler.Handle(request, CancellationToken.None);

        result.TotalTasks.Should().Be(50);
        (result.CompletedTasks + result.PendingTasks).Should().Be(50);
    }

    [Fact]
    public async Task Given_DailyReportWithAllCompleted_When_HandleCalled_Then_ReportAccurate()
    {
        var today = DateTime.Now.Date;
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var report = new KitchenDailyReportResponse
        {
            Date = today,
            TotalTasks = 10,
            CompletedTasks = 10,
            PendingTasks = 0
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetDailyReportAsync(today))
            .ReturnsAsync(report);
        
        var handler = new GetDailyReportHandler(mockKitchenTaskRepository.Object);
        var request = new GetDailyReportRequest(today);

        var result = await handler.Handle(request, CancellationToken.None);

        result.PendingTasks.Should().Be(0);
        result.CompletedTasks.Should().Be(10);
    }
}
