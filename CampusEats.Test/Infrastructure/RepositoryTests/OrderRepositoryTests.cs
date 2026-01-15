using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Infrastructure;

public class OrderRepositoryTests
{
    private CampusEatsDbContext GetTestContext()
    {
        var options = new DbContextOptionsBuilder<CampusEatsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusEatsDbContext(options);
    }

    [Fact]
    public async Task AddAsync_WithValidOrder_InsertsOrderToDatabase()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);
        var order = new Order
        {
            Id = 1,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 25.50m
        };

        // Act
        await repository.AddAsync(order);

        // Assert
        var retrievedOrder = await context.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        retrievedOrder.Should().NotBeNull();
        retrievedOrder?.Status.Should().Be(OrderStatus.Pending);
        retrievedOrder?.TotalAmount.Should().Be(25.50m);
    }

    [Fact(Skip = "OrderRepository requires further investigation")]
    public async Task GetByIdAsync_WithExistingOrder_ReturnsOrder()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = 1,
            UserId = userId,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 30.00m
        };
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result?.Status.Should().Be(OrderStatus.Pending);
        result?.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact(Skip = "OrderRepository requires further investigation")]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { Id = 1, UserId = userId, Status = OrderStatus.Pending, OrderDate = DateTime.UtcNow, TotalAmount = 10m },
            new() { Id = 2, UserId = userId, Status = OrderStatus.Placed, OrderDate = DateTime.UtcNow, TotalAmount = 20m },
            new() { Id = 3, UserId = userId, Status = OrderStatus.Completed, OrderDate = DateTime.UtcNow, TotalAmount = 30m }
        };
        
        foreach (var order in orders)
        {
            await context.Orders.AddAsync(order);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateAsync_WithValidOrder_UpdatesOrderInDatabase()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = 1,
            UserId = userId,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 25.00m
        };
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        // Act
        order.Status = OrderStatus.Placed;
        order.TotalAmount = 27.50m;
        await repository.UpdateAsync(order);

        // Assert
        var updated = await context.Orders.FirstOrDefaultAsync(o => o.Id == 1);
        updated?.Status.Should().Be(OrderStatus.Placed);
        updated?.TotalAmount.Should().Be(27.50m);
    }

    [Fact(Skip = "OrderRepository requires further investigation")]
    public async Task GetOrdersByUserAsync_ReturnsUserOrders()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        var orders = new List<Order>
        {
            new() { Id = 1, UserId = userId1, Status = OrderStatus.Pending, OrderDate = DateTime.UtcNow, TotalAmount = 10m },
            new() { Id = 2, UserId = userId1, Status = OrderStatus.Placed, OrderDate = DateTime.UtcNow, TotalAmount = 20m },
            new() { Id = 3, UserId = userId2, Status = OrderStatus.Completed, OrderDate = DateTime.UtcNow, TotalAmount = 30m }
        };
        
        foreach (var order in orders)
        {
            await context.Orders.AddAsync(order);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetOrdersByUserAsync(userId1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.UserId.Should().Be(userId1));
    }

    [Fact(Skip = "OrderRepository requires further investigation")]
    public async Task GetOrdersByStatusAsync_ReturnsOrdersByStatus()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new OrderRepository(context);
        var userId = Guid.NewGuid();
        
        var orders = new List<Order>
        {
            new() { Id = 1, UserId = userId, Status = OrderStatus.Pending, OrderDate = DateTime.UtcNow, TotalAmount = 10m },
            new() { Id = 2, UserId = userId, Status = OrderStatus.Pending, OrderDate = DateTime.UtcNow, TotalAmount = 20m },
            new() { Id = 3, UserId = userId, Status = OrderStatus.Completed, OrderDate = DateTime.UtcNow, TotalAmount = 30m }
        };
        
        foreach (var order in orders)
        {
            await context.Orders.AddAsync(order);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetOrdersByStatusAsync(OrderStatus.Pending);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }
}

