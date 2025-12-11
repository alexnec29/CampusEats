using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.MenuItem;

public class GetAllMenuItemsHandlerTests
{
    [Fact]
    public async Task Given_NoItemsInDatabase_When_HandleIsCalled_Then_EmptyListReturned()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        mockMenuItemRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Api.Models.MenuItem>());
        
        var handler = new GetAllMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new GetAllMenuItemsRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MultipleItems_When_HandleIsCalled_Then_AllItemsReturned()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Item1" },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Item2" },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Item3" }
        };
        mockMenuItemRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new GetAllMenuItemsRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Given_ItemsWithDifferentCategories_When_HandleIsCalled_Then_AllCategoriesPresent()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Burger", Category = MenuItemCategory.FastFood },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Salad", Category = MenuItemCategory.Salad },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Cake", Category = MenuItemCategory.Dessert }
        };
        mockMenuItemRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new GetAllMenuItemsRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(i => i.Category).Should().Contain(new[] { MenuItemCategory.FastFood, MenuItemCategory.Salad, MenuItemCategory.Dessert });
    }

    [Fact]
    public async Task Given_ItemsWithDifferentPrices_When_HandleIsCalled_Then_PricesPreserved()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Expensive", Price = 99.99m },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Cheap", Price = 1.99m }
        };
        mockMenuItemRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new GetAllMenuItemsRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Contain(i => i.Price == 99.99m);
        result.Should().Contain(i => i.Price == 1.99m);
    }

    [Fact]
    public async Task Given_AvailableAndUnavailableItems_When_HandleIsCalled_Then_StatusPreserved()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Available", IsAvailable = true },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Unavailable", IsAvailable = false }
        };
        mockMenuItemRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new GetAllMenuItemsRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Contain(i => i.IsAvailable == true);
        result.Should().Contain(i => i.IsAvailable == false);
    }

    [Fact]
    public async Task Given_ItemsWithImages_When_HandleIsCalled_Then_ImageUrlsPreserved()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "WithImage", ImageUrl = "https://example.com/image.jpg" },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "WithoutImage", ImageUrl = "" }
        };
        mockMenuItemRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new GetAllMenuItemsRequest();

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Contain(i => i.ImageUrl == "https://example.com/image.jpg");
    }
}

public class DeleteMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemId_When_HandleIsCalled_Then_ItemIsDeleted()
    {
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var item = new Api.Models.MenuItem { Id = menuItemId, Name = "Item" };
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync(item);
        
        var handler = new DeleteMenuItemHandler(mockMenuItemRepository.Object);
        var request = new DeleteMenuItemRequest(menuItemId);

        var result = await handler.Handle(request, CancellationToken.None);

        mockMenuItemRepository.Verify(repo => repo.DeleteAsync(item), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentMenuItemId_When_HandleIsCalled_Then_NothingIsDeleted()
    {
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync((Api.Models.MenuItem)null);
        
        var handler = new DeleteMenuItemHandler(mockMenuItemRepository.Object);
        var request = new DeleteMenuItemRequest(menuItemId);

        var result = await handler.Handle(request, CancellationToken.None);

        mockMenuItemRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Api.Models.MenuItem>()), Times.Never);
    }

    [Fact]
    public async Task Given_MenuItemToDelete_When_HandleIsCalled_Then_RepositoryCalledOnce()
    {
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var item = new Api.Models.MenuItem { Id = menuItemId };
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync(item);
        
        var handler = new DeleteMenuItemHandler(mockMenuItemRepository.Object);
        var request = new DeleteMenuItemRequest(menuItemId);

        await handler.Handle(request, CancellationToken.None);

        mockMenuItemRepository.Verify(repo => repo.DeleteAsync(It.Is<Api.Models.MenuItem>(m => m.Id == menuItemId)), Times.Once);
    }
}

public class GetMenuItemsByCategoryHandlerTests
{
    [Fact]
    public async Task Given_ValidCategory_When_HandleIsCalled_Then_ItemsWithCategoryReturned()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Burger", Category = MenuItemCategory.FastFood },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Fries", Category = MenuItemCategory.FastFood }
        };
        
        mockMenuItemRepository.Setup(repo => repo.GetByCategoryAsync(MenuItemCategory.FastFood))
            .ReturnsAsync(items);
        
        var handler = new GetMenuItemsByCategoryHandler(mockMenuItemRepository.Object);
        var request = new GetMenuItemsByCategoryRequest(MenuItemCategory.FastFood);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(2);
        result.All(i => i.Category == MenuItemCategory.FastFood).Should().BeTrue();
    }

    [Fact]
    public async Task Given_CategoryWithNoItems_When_HandleIsCalled_Then_EmptyListReturned()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        mockMenuItemRepository.Setup(repo => repo.GetByCategoryAsync(MenuItemCategory.Dessert))
            .ReturnsAsync(new List<Api.Models.MenuItem>());
        
        var handler = new GetMenuItemsByCategoryHandler(mockMenuItemRepository.Object);
        var request = new GetMenuItemsByCategoryRequest(MenuItemCategory.Dessert);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_DifferentCategories_When_HandleIsCalled_Then_OnlyRequestedCategoryReturned()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var fastFoodItems = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Burger", Category = MenuItemCategory.FastFood }
        };
        
        mockMenuItemRepository.Setup(repo => repo.GetByCategoryAsync(MenuItemCategory.FastFood))
            .ReturnsAsync(fastFoodItems);
        mockMenuItemRepository.Setup(repo => repo.GetByCategoryAsync(MenuItemCategory.Salad))
            .ReturnsAsync(new List<Api.Models.MenuItem>());
        
        var handler = new GetMenuItemsByCategoryHandler(mockMenuItemRepository.Object);
        var request = new GetMenuItemsByCategoryRequest(MenuItemCategory.FastFood);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Burger");
    }
}
