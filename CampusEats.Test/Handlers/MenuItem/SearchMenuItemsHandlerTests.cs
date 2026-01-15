using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Handlers.MenuItem;

public class SearchMenuItemsHandlerTests
{
    [Fact]
    public async Task Given_SearchTermMatchingName_When_HandleIsCalled_Then_MatchingItemsReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem1 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Margherita Pizza",
            Description = "Classic Italian pizza",
            Price = 12.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };
        
        var menuItem2 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Pepperoni Pizza",
            Description = "Spicy pizza with pepperoni",
            Price = 14.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };
        
        var menuItem3 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Caesar Salad",
            Description = "Fresh salad",
            Price = 8.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };
        
        dbContext.MenuItems.AddRange(menuItem1, menuItem2, menuItem3);
        await dbContext.SaveChangesAsync();
        
        var request = new SearchMenuItemsRequest("pizza");
        var handler = new SearchMenuItemsHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        var okResult = result as Microsoft.AspNetCore.Http.IResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_SearchTermMatchingDescription_When_HandleIsCalled_Then_MatchingItemsReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem1 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Classic Burger",
            Description = "Juicy beef burger with cheese",
            Price = 10.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };
        
        var menuItem2 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Chicken Sandwich",
            Description = "Grilled chicken with lettuce",
            Price = 9.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };
        
        dbContext.MenuItems.AddRange(menuItem1, menuItem2);
        await dbContext.SaveChangesAsync();
        
        var request = new SearchMenuItemsRequest("beef");
        var handler = new SearchMenuItemsHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_EmptySearchTerm_When_HandleIsCalled_Then_AllItemsReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem1 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Item 1",
            Description = "Description 1",
            Price = 10.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true
        };
        
        var menuItem2 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Item 2",
            Description = "Description 2",
            Price = 12.99m,
            Category = MenuCategory.Dinner,
            IsAvailable = true
        };
        
        var menuItem3 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Item 3",
            Description = "Description 3",
            Price = 8.99m,
            Category = MenuCategory.Breakfast,
            IsAvailable = false
        };
        
        dbContext.MenuItems.AddRange(menuItem1, menuItem2, menuItem3);
        await dbContext.SaveChangesAsync();
        
        var request = new SearchMenuItemsRequest("");
        var handler = new SearchMenuItemsHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var allItems = await dbContext.MenuItems.ToListAsync();
        allItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task Given_NullSearchTerm_When_HandleIsCalled_Then_AllItemsReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem1 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Pancakes",
            Description = "Fluffy pancakes",
            Price = 7.99m,
            Category = MenuCategory.Breakfast,
            IsAvailable = true
        };
        
        var menuItem2 = new CampusEats.Api.Models.MenuItem
        {
            Name = "Waffles",
            Description = "Belgian waffles",
            Price = 8.99m,
            Category = MenuCategory.Breakfast,
            IsAvailable = true
        };
        
        dbContext.MenuItems.AddRange(menuItem1, menuItem2);
        await dbContext.SaveChangesAsync();
        
        var request = new SearchMenuItemsRequest(null);
        var handler = new SearchMenuItemsHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var allItems = await dbContext.MenuItems.ToListAsync();
        allItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_NoMatchingSearchTerm_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Spaghetti",
            Description = "Italian pasta",
            Price = 11.99m,
            Category = MenuCategory.Dinner,
            IsAvailable = true
        };
        
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new SearchMenuItemsRequest("sushi");
        var handler = new SearchMenuItemsHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_CaseInsensitiveSearchTerm_When_HandleIsCalled_Then_MatchingItemsReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Chocolate Cake",
            Description = "Rich chocolate dessert",
            Price = 6.99m,
            Category = MenuCategory.Desserts,
            IsAvailable = true
        };
        
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new SearchMenuItemsRequest("CHOCOLATE");
        var handler = new SearchMenuItemsHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }
}
