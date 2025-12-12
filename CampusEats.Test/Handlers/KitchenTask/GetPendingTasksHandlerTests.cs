using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.KitchenTask;

public class GetPendingTasksHandlerTests
{
    [Fact]
    public async Task Given_PendingAndPreparingTasks_When_HandleIsCalled_Then_OnlyPendingTasksReturned()
    {
        //Arrange
        var tasks = new List<Api.Models.KitchenTask>
        {
            new Api.Models.KitchenTask 
            { 
                Id = 1, 
                OrderId = 1, 
                Status = OrderStatus.Pending, 
                CreatedAt = DateTime.UtcNow.AddHours(-2) 
            },
            new Api.Models.KitchenTask 
            { 
                Id = 2, 
                OrderId = 2, 
                Status = OrderStatus.Preparing, 
                CreatedAt = DateTime.UtcNow.AddHours(-1) 
            },
            new Api.Models.KitchenTask 
            { 
                Id = 3, 
                OrderId = 3, 
                Status = OrderStatus.Completed, 
                CreatedAt = DateTime.UtcNow 
            }
        };
        
        GetPendingTasksQuery query = new GetPendingTasksQuery();
        Mock<IKitchenTaskRepository> mockedRepository = new Mock<IKitchenTaskRepository>();
        
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(tasks);
        
        GetPendingTasksHandler handler = new GetPendingTasksHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<List<KitchenTaskResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().HaveCount(2);
        okResult.Value.Should().OnlyContain(t => 
            t.Status == OrderStatus.Pending || t.Status == OrderStatus.Preparing);
    }
    
    [Fact]
    public async Task Given_NoTasks_When_HandleIsCalled_Then_EmptyListReturned()
    {
        //Arrange
        var emptyList = new List<Api.Models.KitchenTask>();
        
        GetPendingTasksQuery query = new GetPendingTasksQuery();
        Mock<IKitchenTaskRepository> mockedRepository = new Mock<IKitchenTaskRepository>();
        
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(emptyList);
        
        GetPendingTasksHandler handler = new GetPendingTasksHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<List<KitchenTaskResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().BeEmpty();
    }
}
