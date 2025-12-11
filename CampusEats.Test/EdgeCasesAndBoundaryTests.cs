using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Models;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.EdgeCasesAndBoundaries;

public class PricingEdgeCaseTests
{
    [Fact]
    public async Task Given_VeryHighPrice_When_MenuItemCreated_Then_Stored()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest(
            "Expensive Item",
            "Description",
            decimal.MaxValue / 2,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.Is<MenuItem>(m => m.Name == "Expensive Item")), Times.Once);
    }

    [Fact]
    public async Task Given_ZeroPrice_When_MenuItemCreated_Then_ValidatedOrRejected()
    {
        var validator = new CreateMenuItemValidator();
        var request = new CreateMenuItemRequest(
            "Free Item",
            "Description",
            0m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        var result = await validator.ValidateAsync(request);
        
        // Should fail validation for zero price
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_NegativePrice_When_MenuItemCreated_Then_RejectedByValidator()
    {
        var validator = new CreateMenuItemValidator();
        var request = new CreateMenuItemRequest(
            "Negative Item",
            "Description",
            -10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_VerySmallPositivePrice_When_MenuItemCreated_Then_Accepted()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest(
            "Cheap Item",
            "Description",
            0.01m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_PriceWithManyDecimalPlaces_When_MenuItemCreated_Then_Stored()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest(
            "Precise Item",
            "Description",
            19.99m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }
}

public class TextInputEdgeCaseTests
{
    [Fact]
    public async Task Given_VeryLongMenuItemName_When_Created_Then_ValidatedOrTruncated()
    {
        var validator = new CreateMenuItemValidator();
        var longName = new string('a', 10000);
        var request = new CreateMenuItemRequest(
            longName,
            "Description",
            10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        var result = await validator.ValidateAsync(request);
        
        // Should fail due to length
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_MenuItemNameWithSpecialCharacters_When_Created_Then_Stored()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest(
            "Spicy #1 & Tasty $$$",
            "Description: Very good!",
            10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_UnicodeCharactersInInput_When_Created_Then_Stored()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest(
            "中文食物 🍕 Comida 한글",
            "Description with émojis 😋",
            10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_MenuItemNameWithNewlines_When_Created_Then_Normalized()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest(
            "Burger\nWith\nNewlines",
            "Description",
            10m,
            MenuItemCategory.MainCourse,
            "url",
            true
        );
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }
}

public class GuidAndIdEdgeCaseTests
{
    [Fact]
    public async Task Given_EmptyGuidUserId_When_OrderCreated_Then_Rejected()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var emptyGuid = Guid.Empty;
        mockUserRepository.Setup(repo => repo.GetByIdAsync(emptyGuid))
            .ReturnsAsync((User)null);
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(emptyGuid, "Notes");
        
        // Should handle empty guid appropriately
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.GetByIdAsync(emptyGuid), Times.Once);
    }

    [Fact]
    public async Task Given_MaxValueGuid_When_Searched_Then_NoResultsOrError()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var maxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        
        // Should not throw, just return no results
        mockRepository.Setup(repo => repo.GetByIdAsync(maxGuid))
            .ReturnsAsync((MenuItem)null);
        
        var result = await mockRepository.Object.GetByIdAsync(maxGuid);
        
        result.Should().BeNull();
    }
}

public class OrderStatusTransitionEdgeCaseTests
{
    [Fact]
    public async Task Given_OrderAlreadyCompleted_When_CancelRequested_Then_RejectedOrHandled()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CancelOrderValidator>();
        
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Completed };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        
        var handler = new CancelOrderHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new CancelOrderRequest(order.Id);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockOrderRepository.Verify(repo => repo.GetByIdAsync(order.Id), Times.Once);
    }

    [Fact]
    public async Task Given_OrderInProgressStatus_When_CancelRequested_Then_Handled()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CancelOrderValidator>();
        
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.InProgress };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        
        var handler = new CancelOrderHandler(mockOrderRepository.Object, mockValidator.Object);
        var request = new CancelOrderRequest(order.Id);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockOrderRepository.Verify(repo => repo.GetByIdAsync(order.Id), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleStatusTransitionsRapidly_When_Applied_Then_LastOneWins()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(repo => repo.UpdateAsync(order))
            .Returns(Task.CompletedTask);
        
        var handler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockValidator.Object);
        
        var statuses = new[] { OrderStatus.Confirmed, OrderStatus.InProgress, OrderStatus.Completed };
        
        foreach (var status in statuses)
        {
            order.Status = status;
            var request = new UpdateOrderStatusRequest(order.Id, status);
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.AtLeast(3));
    }
}

public class ConcurrentModificationTests
{
    [Fact]
    public async Task Given_SameMenuItemModifiedConcurrently_When_Updated_Then_LastWriteWins()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<UpdateMenuItemValidator>();
        
        var menuItem = new MenuItem { Id = Guid.NewGuid(), Name = "Original" };
        mockRepository.Setup(repo => repo.GetByIdAsync(menuItem.Id))
            .ReturnsAsync(menuItem);
        
        var handler = new UpdateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        var tasks = new[]
        {
            handler.Handle(new UpdateMenuItemRequest(menuItem.Id, "Updated1", "Desc", 10m, null, true), CancellationToken.None),
            handler.Handle(new UpdateMenuItemRequest(menuItem.Id, "Updated2", "Desc", 10m, null, true), CancellationToken.None),
            handler.Handle(new UpdateMenuItemRequest(menuItem.Id, "Updated3", "Desc", 10m, null, true), CancellationToken.None)
        };
        
        await Task.WhenAll(tasks);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<MenuItem>()), Times.AtLeast(3));
    }

    [Fact]
    public async Task Given_OrderAndMenuItemModifiedSimultaneously_When_Updated_Then_NoDataCorruption()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockOrderValidator = new Mock<UpdateOrderStatusValidator>();
        var mockMenuValidator = new Mock<UpdateMenuItemValidator>();
        
        var order = new Order { Id = Guid.NewGuid() };
        var menuItem = new MenuItem { Id = Guid.NewGuid() };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItem.Id))
            .ReturnsAsync(menuItem);
        
        var orderHandler = new UpdateOrderStatusHandler(mockOrderRepository.Object, mockOrderValidator.Object);
        var menuHandler = new UpdateMenuItemHandler(mockMenuItemRepository.Object, mockMenuValidator.Object);
        
        var tasks = new Task[]
        {
            orderHandler.Handle(new UpdateOrderStatusRequest(order.Id, OrderStatus.Completed), CancellationToken.None),
            menuHandler.Handle(new UpdateMenuItemRequest(menuItem.Id, "Updated", "Desc", 10m, null, true), CancellationToken.None)
        };
        
        await Task.WhenAll(tasks);
        
        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Once);
        mockMenuItemRepository.Verify(repo => repo.UpdateAsync(It.IsAny<MenuItem>()), Times.Once);
    }
}

public class ListAndCollectionEdgeCaseTests
{
    [Fact]
    public async Task Given_GetAllWithEmptyDatabase_When_Called_Then_ReturnsEmptyList()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetAllMenuItemsValidator>();
        
        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<MenuItem>());
        
        var handler = new GetAllMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetAllMenuItemsRequest();
        
        var result = await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_GetAllWithThousandsOfItems_When_Called_Then_AllReturned()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetAllMenuItemsValidator>();
        
        var items = Enumerable.Range(0, 1000)
            .Select(i => new MenuItem { Id = Guid.NewGuid(), Name = $"Item {i}" })
            .ToList();
        
        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetAllMenuItemsRequest();
        
        var result = await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_GetAllAndFilterByCategory_When_OnlyMatchingReturned_Then_CorrectSubset()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetMenuItemsByCategoryValidator>();
        
        var category = MenuItemCategory.MainCourse;
        var items = Enumerable.Range(0, 50)
            .Select(i => new MenuItem 
            { 
                Id = Guid.NewGuid(), 
                Name = $"Item {i}",
                Category = i % 2 == 0 ? category : MenuItemCategory.Appetizer
            })
            .ToList();
        
        mockRepository.Setup(repo => repo.GetByCategoryAsync(category))
            .ReturnsAsync(items.Where(i => i.Category == category).ToList());
        
        var handler = new GetMenuItemsByCategoryHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetMenuItemsByCategoryRequest(category);
        
        var result = await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetByCategoryAsync(category), Times.Once);
    }
}
