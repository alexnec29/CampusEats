using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.KitchenTask;

public class AssignTaskToStaffHandlerTests
{
    [Fact]
    public async Task Given_NonExistentTask_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int nonExistentTaskId = 999;
        Guid staffId = Guid.NewGuid();
        AssignTaskToStaffCommand command = new AssignTaskToStaffCommand(nonExistentTaskId, staffId);
        
        Mock<IKitchenTaskRepository> mockedTaskRepo = new Mock<IKitchenTaskRepository>();
        Mock<IUserRepository> mockedUserRepo = new Mock<IUserRepository>();
        
        mockedTaskRepo.Setup(r => r.GetByIdAsync(nonExistentTaskId))
            .ReturnsAsync((Api.Models.KitchenTask?)null);
        
        AssignTaskToStaffHandler handler = new AssignTaskToStaffHandler(
            mockedTaskRepo.Object,
            mockedUserRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonExistentStaff_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        int taskId = 1;
        Guid nonExistentStaffId = Guid.NewGuid();
        AssignTaskToStaffCommand command = new AssignTaskToStaffCommand(taskId, nonExistentStaffId);
        
        var task = new Api.Models.KitchenTask
        {
            Id = taskId,
            OrderId = 1,
            Status = OrderStatus.Pending
        };
        
        Mock<IKitchenTaskRepository> mockedTaskRepo = new Mock<IKitchenTaskRepository>();
        Mock<IUserRepository> mockedUserRepo = new Mock<IUserRepository>();
        
        mockedTaskRepo.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        mockedUserRepo.Setup(r => r.GetByIdAsync(nonExistentStaffId))
            .ReturnsAsync((Api.Models.User?)null);
        
        AssignTaskToStaffHandler handler = new AssignTaskToStaffHandler(
            mockedTaskRepo.Object,
            mockedUserRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_ValidTaskAndStaff_When_HandleIsCalled_Then_TaskAssignedAndStatusUpdated()
    {
        //Arrange
        int taskId = 1;
        Guid staffId = Guid.NewGuid();
        AssignTaskToStaffCommand command = new AssignTaskToStaffCommand(taskId, staffId);
        
        var task = new Api.Models.KitchenTask
        {
            Id = taskId,
            OrderId = 1,
            Status = OrderStatus.Pending
        };
        
        var staff = new Api.Models.User
        {
            Id = staffId,
            Email = "staff@test.com",
            Username = "staffmember"
        };
        
        Mock<IKitchenTaskRepository> mockedTaskRepo = new Mock<IKitchenTaskRepository>();
        Mock<IUserRepository> mockedUserRepo = new Mock<IUserRepository>();
        
        mockedTaskRepo.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        
        mockedUserRepo.Setup(r => r.GetByIdAsync(staffId))
            .ReturnsAsync(staff);
        
        AssignTaskToStaffHandler handler = new AssignTaskToStaffHandler(
            mockedTaskRepo.Object,
            mockedUserRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<Api.Models.KitchenTask>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        
        Assert.Equal(staffId, task.AssignedStaffId);
        Assert.Equal(OrderStatus.Preparing, task.Status); // Status should change from Pending to Preparing
        
        mockedTaskRepo.Verify(r => r.UpdateAsync(task), Times.Once);
    }
}
