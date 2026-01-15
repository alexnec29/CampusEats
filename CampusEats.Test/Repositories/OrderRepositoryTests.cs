using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class OrderRepositoryTests
{
    [Fact]
    public async Task Given_ValidOrder_When_AddAsyncCalled_Then_OrderAdded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            Status = OrderStatus.Pending,
            TotalAmount = 25.00m
        };

        // Act
        await repository.AddAsync(order);

        // Assert
        var savedOrder = await repository.GetByIdAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.UserId.Should().Be(user.Id);
        savedOrder.TotalAmount.Should().Be(25.00m);
    }

    [Fact]
    public async Task Given_ExistingOrder_When_GetByIdAsyncCalled_Then_OrderWithRelationsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var menuItem = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();

        var order = new Order { UserId = user.Id, Status = OrderStatus.Pending, TotalAmount = 10.00m };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var orderItem = new OrderItem { OrderId = order.Id, MenuItemId = menuItem.Id, Quantity = 1 };
        dbContext.OrderItems.Add(orderItem);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.OrderItems.Should().HaveCount(1);
        result.OrderItems.First().MenuItem.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_GetByIdAsyncCalled_Then_NullReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_MultipleOrders_When_GetOrdersByUserAsyncCalled_Then_OnlyUserOrdersReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user1 = new User { Username = "buyer1", Email = "buyer1@test.com", HashedPassword = "hash", Role = Role.Buyer };
        var user2 = new User { Username = "buyer2", Email = "buyer2@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.AddRange(user1, user2);
        await dbContext.SaveChangesAsync();

        var order1 = new Order { UserId = user1.Id, Status = OrderStatus.Pending, TotalAmount = 10.00m };
        var order2 = new Order { UserId = user1.Id, Status = OrderStatus.Completed, TotalAmount = 20.00m };
        var order3 = new Order { UserId = user2.Id, Status = OrderStatus.Pending, TotalAmount = 15.00m };
        dbContext.Orders.AddRange(order1, order2, order3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetOrdersByUserAsync(user1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.UserId == user1.Id);
    }

    [Fact]
    public async Task Given_MultipleOrders_When_GetOrdersByStatusAsyncCalled_Then_OnlyMatchingStatusReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var order1 = new Order { UserId = user.Id, Status = OrderStatus.Pending, TotalAmount = 10.00m };
        var order2 = new Order { UserId = user.Id, Status = OrderStatus.Completed, TotalAmount = 20.00m };
        var order3 = new Order { UserId = user.Id, Status = OrderStatus.Pending, TotalAmount = 15.00m };
        dbContext.Orders.AddRange(order1, order2, order3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetOrdersByStatusAsync(OrderStatus.Pending);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.Status == OrderStatus.Pending);
    }

    [Fact]
    public async Task Given_ExistingOrder_When_UpdateAsyncCalled_Then_OrderUpdated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var order = new Order { UserId = user.Id, Status = OrderStatus.Pending, TotalAmount = 10.00m };
        await repository.AddAsync(order);

        // Act
        order.Status = OrderStatus.Completed;
        order.TotalAmount = 15.00m;
        await repository.UpdateAsync(order);

        // Assert
        var updatedOrder = await repository.GetByIdAsync(order.Id);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);
        updatedOrder.TotalAmount.Should().Be(15.00m);
    }

    [Fact]
    public async Task Given_ExistingOrder_When_DeleteAsyncCalled_Then_OrderDeleted()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var order = new Order { UserId = user.Id, Status = OrderStatus.Pending, TotalAmount = 10.00m };
        await repository.AddAsync(order);
        var orderId = order.Id;

        // Act
        await repository.DeleteAsync(orderId);

        // Assert
        var deletedOrder = await repository.GetByIdAsync(orderId);
        deletedOrder.Should().BeNull();
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_DeleteAsyncCalled_Then_NoExceptionThrown()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        // Act
        var act = async () => await repository.DeleteAsync(999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_OrderWithKitchenTask_When_GetByIdAsyncCalled_Then_KitchenTaskIncluded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var order = new Order { UserId = user.Id, Status = OrderStatus.Pending, TotalAmount = 10.00m };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var task = new KitchenTask { OrderId = order.Id, Status = OrderStatus.Preparing };
        dbContext.KitchenTasks.Add(task);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.KitchenTask.Should().NotBeNull();
        result.KitchenTask!.Status.Should().Be(OrderStatus.Preparing);
    }

    [Fact]
    public async Task Given_NoOrders_When_GetAllAsyncCalled_Then_EmptyListReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new OrderRepository(dbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
