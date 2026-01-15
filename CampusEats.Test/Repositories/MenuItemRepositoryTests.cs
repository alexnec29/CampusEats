using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class MenuItemRepositoryTests
{
    [Fact]
    public async Task Given_ValidMenuItem_When_AddAsyncCalled_Then_MenuItemAdded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var menuItem = new MenuItem
        {
            Name = "Pizza",
            Price = 12.50m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };

        // Act
        await repository.AddAsync(menuItem);

        // Assert
        var savedItem = await repository.GetByIdAsync(menuItem.Id);
        savedItem.Should().NotBeNull();
        savedItem!.Name.Should().Be("Pizza");
        savedItem.Price.Should().Be(12.50m);
    }

    [Fact]
    public async Task Given_AvailableMenuItems_When_GetAvailableMenuItemsAsyncCalled_Then_OnlyAvailableReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var item1 = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        var item2 = new MenuItem { Name = "Burger", Price = 8.00m, Category = MenuCategory.Lunch, IsAvailable = false };
        var item3 = new MenuItem { Name = "Salad", Price = 6.00m, Category = MenuCategory.Breakfast, IsAvailable = true };
        dbContext.MenuItems.AddRange(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetAvailableMenuItemsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.IsAvailable);
        result.Should().Contain(m => m.Name == "Pizza");
        result.Should().Contain(m => m.Name == "Salad");
    }

    [Fact]
    public async Task Given_MenuItemsByCategory_When_GetMenuItemsByCategoryAsyncCalled_Then_OnlyCategoryItemsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var item1 = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        var item2 = new MenuItem { Name = "Cake", Price = 5.00m, Category = MenuCategory.Desserts, IsAvailable = true };
        var item3 = new MenuItem { Name = "Pasta", Price = 12.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        dbContext.MenuItems.AddRange(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetMenuItemsByCategoryAsync(MenuCategory.Lunch);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.Category == MenuCategory.Lunch);
    }

    [Fact]
    public async Task Given_ExistingMenuItem_When_UpdateAsyncCalled_Then_MenuItemUpdated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var menuItem = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        await repository.AddAsync(menuItem);

        // Act
        menuItem.Price = 12.00m;
        menuItem.IsAvailable = false;
        await repository.UpdateAsync(menuItem);

        // Assert
        var updated = await repository.GetByIdAsync(menuItem.Id);
        updated.Should().NotBeNull();
        updated!.Price.Should().Be(12.00m);
        updated.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task Given_ExistingMenuItem_When_DeleteAsyncCalled_Then_MenuItemDeleted()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var menuItem = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        await repository.AddAsync(menuItem);
        var itemId = menuItem.Id;

        // Act
        await repository.DeleteAsync(itemId);

        // Assert
        var deleted = await repository.GetByIdAsync(itemId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Given_NonExistentMenuItem_When_GetByIdAsyncCalled_Then_NullReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_EmptyCategory_When_GetMenuItemsByCategoryAsyncCalled_Then_EmptyListReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var item = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        dbContext.MenuItems.Add(item);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetMenuItemsByCategoryAsync(MenuCategory.Drinks);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_NoAvailableItems_When_GetAvailableMenuItemsAsyncCalled_Then_EmptyListReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var item = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = false };
        dbContext.MenuItems.Add(item);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetAvailableMenuItemsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MultipleItems_When_GetAllAsyncCalled_Then_AllItemsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new MenuItemRepository(dbContext);

        var item1 = new MenuItem { Name = "Pizza", Price = 10.00m, Category = MenuCategory.Lunch, IsAvailable = true };
        var item2 = new MenuItem { Name = "Cake", Price = 5.00m, Category = MenuCategory.Desserts, IsAvailable = false };
        dbContext.MenuItems.AddRange(item1, item2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
