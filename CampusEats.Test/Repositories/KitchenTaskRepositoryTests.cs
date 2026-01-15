using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class KitchenTaskRepositoryTests
{
    [Fact]
    public async Task Given_ValidTask_When_AddAsyncCalled_Then_TaskAdded()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new KitchenTaskRepository(dbContext);
        var task = new KitchenTask
        {
            Status = OrderStatus.Pending,
            OrderId = 1,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(task);

        var savedTask = await repository.GetByIdAsync(task.Id);
        savedTask.Should().NotBeNull();
        savedTask!.Status.Should().Be(OrderStatus.Pending);
        savedTask.OrderId.Should().Be(1);
    }

    [Fact]
    public async Task Given_TasksWithDifferentStatuses_When_GetByStatusAsyncCalled_Then_ReturnsFilteredTasks()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new KitchenTaskRepository(dbContext);
        await repository.AddAsync(new KitchenTask { Status = OrderStatus.Pending, OrderId = 1 });
        await repository.AddAsync(new KitchenTask { Status = OrderStatus.Completed, OrderId = 2 });
        await repository.AddAsync(new KitchenTask { Status = OrderStatus.Pending, OrderId = 3 });

        var result = await repository.GetByStatusAsync(OrderStatus.Pending);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Status == OrderStatus.Pending);
    }

    [Fact]
    public async Task Given_TaskForStaff_When_GetByStaffIdAsyncCalled_Then_ReturnsStaffTasks()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new KitchenTaskRepository(dbContext);
        var staffId = Guid.NewGuid();
        
        await repository.AddAsync(new KitchenTask { AssignedStaffId = staffId, OrderId = 1 });
        await repository.AddAsync(new KitchenTask { AssignedStaffId = Guid.NewGuid(), OrderId = 2 });

        var result = await repository.GetByStaffIdAsync(staffId);

        result.Should().HaveCount(1);
        result.First().AssignedStaffId.Should().Be(staffId);
    }

    [Fact]
    public async Task Given_TaskForOrder_When_GetByOrderIdAsyncCalled_Then_ReturnsCorrectTask()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new KitchenTaskRepository(dbContext);
        var targetOrderId = 123;
        
        await repository.AddAsync(new KitchenTask { OrderId = targetOrderId, Status = OrderStatus.Pending });
        await repository.AddAsync(new KitchenTask { OrderId = 456, Status = OrderStatus.Pending });

        var result = await repository.GetByOrderIdAsync(targetOrderId);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(targetOrderId);
    }
}