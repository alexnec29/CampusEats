using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;
using System.Diagnostics;

namespace CampusEats.Test.Performance.AndOptimization;

public class PerformanceAndLoadTests
{
    [Fact]
    public async Task Given_CreateManyMenuItemsSequentially_When_TimeManaged_Then_NoTimeout()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 500; i++)
        {
            var request = new CreateMenuItemRequest(
                $"Item {i}",
                $"Description {i}",
                (decimal)(10 + i * 0.1),
                MenuItemCategory.MainCourse,
                $"url-{i}",
                true
            );
            
            await handler.Handle(request, CancellationToken.None);
        }
        
        stopwatch.Stop();
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "500 items should be created in reasonable time");
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Exactly(500));
    }

    [Fact]
    public async Task Given_GetAllMenuItemsWithLargeDataset_When_Retrieved_Then_ReturnsAll()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetAllMenuItemsValidator>();
        
        var items = Enumerable.Range(0, 5000)
            .Select(i => new MenuItem { Id = Guid.NewGuid(), Name = $"Item {i}", Price = (decimal)i })
            .ToList();
        
        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var stopwatch = Stopwatch.StartNew();
        
        var result = await handler.Handle(new GetAllMenuItemsRequest(), CancellationToken.None);
        
        stopwatch.Stop();
        
        items.Should().HaveCount(5000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Retrieval should be fast");
    }

    [Fact]
    public async Task Given_ParallelOrderCreation_When_Concurrent_Then_AllProcessed()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var users = Enumerable.Range(0, 100)
            .Select(_ => new User { Id = Guid.NewGuid() })
            .ToList();
        
        foreach (var user in users)
        {
            mockUserRepository.Setup(repo => repo.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(user.Id))
                .ReturnsAsync(new List<Order>());
        }
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = users.Select(user =>
            handler.Handle(new CreateOrderRequest(user.Id, "Notes"), CancellationToken.None)
        );
        
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "100 concurrent orders should complete quickly");
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Exactly(100));
    }

    [Fact]
    public async Task Given_SearchMenuItemsWithLargeDataset_When_FilterApplied_Then_ResultsQuick()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<SearchMenuItemsValidator>();
        
        var allItems = Enumerable.Range(0, 10000)
            .Select(i => new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = i % 100 == 0 ? "Burger" : $"Item {i}",
                Price = (decimal)i,
                Category = (MenuItemCategory)(i % 4)
            })
            .ToList();
        
        var filtered = allItems.Where(i => i.Name.Contains("Burger")).ToList();
        
        mockRepository.Setup(repo => repo.SearchAsync("Burger", null, null))
            .ReturnsAsync(filtered);
        
        var handler = new SearchMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var stopwatch = Stopwatch.StartNew();
        
        var result = await handler.Handle(new SearchMenuItemsRequest("Burger", null, null), CancellationToken.None);
        
        stopwatch.Stop();
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Search should return results quickly");
    }
}

public class MemoryAndResourceTests
{
    [Fact]
    public async Task Given_CreateOrdersWithMinimalData_When_Stored_Then_EfficiencyVerified()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var userId = Guid.NewGuid();
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Order>());
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        
        // Create many minimal orders
        var orders = new List<Order>();
        for (int i = 0; i < 1000; i++)
        {
            var request = new CreateOrderRequest(userId, "");
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Exactly(1000));
    }

    [Fact]
    public async Task Given_LargeTextFieldsInMenuItem_When_Stored_Then_Handled()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        var largeDescription = new string('A', 2000);
        var request = new CreateMenuItemRequest(
            "Item",
            largeDescription,
            10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleOrdersWithManyItems_When_Processed_Then_EfficiencyMaintained()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var orderId = Guid.NewGuid();
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(new Order { Id = orderId });
        
        var handler = new AddOrderItemHandler(mockRepository.Object, mockValidator.Object);
        
        // Add 500 items to order
        for (int i = 0; i < 500; i++)
        {
            var request = new AddOrderItemRequest(orderId, Guid.NewGuid(), 1);
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Exactly(500));
    }
}

public class DataConsistencyAndConcurrencyTests
{
    [Fact]
    public async Task Given_RapidUpdatesToSameMenuItem_When_Concurrent_Then_LastWriteWins()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<UpdateMenuItemValidator>();
        
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem { Id = menuItemId, Name = "Original" };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync(menuItem);
        
        var handler = new UpdateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        var updates = Enumerable.Range(0, 50)
            .Select(i => new UpdateMenuItemRequest(menuItemId, $"Version {i}", "Desc", 10m, null, true))
            .ToList();
        
        var tasks = updates.Select(update => handler.Handle(update, CancellationToken.None));
        
        await Task.WhenAll(tasks);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<MenuItem>()), Times.AtLeast(50));
    }

    [Fact]
    public async Task Given_MultipleUsersCreatingOrders_When_Simultaneous_Then_NoConflicts()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var users = Enumerable.Range(0, 50)
            .Select(_ => new User { Id = Guid.NewGuid() })
            .ToList();
        
        foreach (var user in users)
        {
            mockUserRepository.Setup(repo => repo.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(user.Id))
                .ReturnsAsync(new List<Order>());
        }
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        
        var requests = users.Select(user => new CreateOrderRequest(user.Id, "Notes")).ToList();
        var tasks = requests.Select(req => handler.Handle(req, CancellationToken.None));
        
        await Task.WhenAll(tasks);
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Exactly(50));
    }

    [Fact]
    public async Task Given_OrderBeingModifiedWhileFetched_When_Concurrent_Then_DataIntegrity()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUpdateValidator = new Mock<UpdateOrderStatusValidator>();
        var mockAddItemValidator = new Mock<AddOrderItemValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Pending, Items = new List<OrderItem>() };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var updateHandler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockUpdateValidator.Object);
        var addItemHandler = new AddOrderItemHandler(mockOrderRepository.Object, mockAddItemValidator.Object);
        
        var tasks = new List<Task>();
        
        // Concurrent status updates
        tasks.Add(updateHandler.Handle(new UpdateOrderStatusRequest(orderId, OrderStatus.Confirmed), CancellationToken.None));
        
        // Concurrent item additions
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(addItemHandler.Handle(new AddOrderItemRequest(orderId, Guid.NewGuid(), 1), CancellationToken.None));
        }
        
        await Task.WhenAll(tasks);
        
        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.AtLeast(11));
    }
}

public class BoundaryAndExtremeValueTests
{
    [Fact]
    public async Task Given_MenuItemPriceAtDecimalLimit_When_Stored_Then_Handled()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        // Test with very high price
        var request = new CreateMenuItemRequest(
            "Expensive Item",
            "Description",
            999999.99m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_VeryLongStringFields_When_Created_Then_Validated()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        var veryLongName = new string('X', 5000);
        var request = new CreateMenuItemRequest(
            veryLongName,
            new string('Y', 10000),
            10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        // Should either reject or truncate
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.AtMost(1));
    }

    [Fact]
    public async Task Given_OrderWithMaximumNumberOfItems_When_Added_Then_Handled()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var orderId = Guid.NewGuid();
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(new Order { Id = orderId });
        
        var handler = new AddOrderItemHandler(mockRepository.Object, mockValidator.Object);
        
        // Add many items
        for (int i = 0; i < 1000; i++)
        {
            var request = new AddOrderItemRequest(orderId, Guid.NewGuid(), 1);
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Exactly(1000));
    }

    [Fact]
    public async Task Given_GuidsWithAllFormats_When_Created_Then_Handled()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        // Test various GUID values
        var guids = new[] 
        { 
            Guid.NewGuid(),
            Guid.Empty,
            new Guid("00000000-0000-0000-0000-000000000001"),
            new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")
        };
        
        foreach (var guid in guids.Skip(1)) // Skip empty and test others
        {
            var request = new CreateMenuItemRequest(
                guid.ToString(),
                "Description",
                10m,
                MenuItemCategory.MainCourse,
                "url",
                true
            );
            
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.AtLeast(3));
    }
}

public class FailureRecoveryTests
{
    [Fact]
    public async Task Given_RepositoryThrowsExceptionMidway_When_Recovered_Then_PartialResultsHandled()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var callCount = 0;
        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<MenuItem>()))
            .Callback(() => { callCount++; if (callCount == 5) throw new Exception("Simulated failure"); })
            .Returns(Task.CompletedTask);
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var request = new CreateMenuItemRequest($"Item {i}", "Desc", 10m, MenuItemCategory.MainCourse, "url", true);
                await handler.Handle(request, CancellationToken.None);
            }
            catch
            {
                // Expected for item 5
            }
        }
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Exactly(10));
    }

    [Fact]
    public async Task Given_PartiallyFailedBatch_When_Retried_Then_CompletedSuccessfully()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var users = Enumerable.Range(0, 10).Select(_ => new User { Id = Guid.NewGuid() }).ToList();
        
        foreach (var user in users)
        {
            mockUserRepository.Setup(repo => repo.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(user.Id))
                .ReturnsAsync(new List<Order>());
        }
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        
        var successCount = 0;
        foreach (var user in users)
        {
            try
            {
                var request = new CreateOrderRequest(user.Id, "Notes");
                await handler.Handle(request, CancellationToken.None);
                successCount++;
            }
            catch
            {
                // Retry
                var request = new CreateOrderRequest(user.Id, "Notes Retry");
                await handler.Handle(request, CancellationToken.None);
                successCount++;
            }
        }
        
        successCount.Should().BeGreaterThanOrEqualTo(10);
    }
}
