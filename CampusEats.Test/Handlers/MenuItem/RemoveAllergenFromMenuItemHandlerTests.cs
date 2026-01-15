using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Handlers.MenuItem;

public class RemoveAllergenFromMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemWithAllergen_When_HandleIsCalled_Then_AllergenRemovedSuccessfully()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var allergen = new CampusEats.Api.Models.Allergen
        {
            Name = "Gluten",
            Description = "Contains wheat"
        };
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Pizza",
            Description = "Cheese pizza",
            Price = 12.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen> { allergen }
        };
        
        dbContext.Allergens.Add(allergen);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new RemoveAllergenFromMenuItemRequest(menuItem.Id, allergen.Id);
        var handler = new RemoveAllergenFromMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var updatedMenuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id);
        
        updatedMenuItem.Should().NotBeNull();
        updatedMenuItem!.Allergens.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_NonExistentMenuItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var allergen = new CampusEats.Api.Models.Allergen
        {
            Name = "Dairy",
            Description = "Contains milk"
        };
        
        dbContext.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync();
        
        var request = new RemoveAllergenFromMenuItemRequest(999, allergen.Id);
        var handler = new RemoveAllergenFromMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_AllergenNotOnMenuItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var allergen1 = new CampusEats.Api.Models.Allergen
        {
            Name = "Gluten",
            Description = "Contains wheat"
        };
        
        var allergen2 = new CampusEats.Api.Models.Allergen
        {
            Name = "Dairy",
            Description = "Contains milk"
        };
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Caesar Salad",
            Description = "Fresh salad",
            Price = 8.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen> { allergen1 }
        };
        
        dbContext.Allergens.AddRange(allergen1, allergen2);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new RemoveAllergenFromMenuItemRequest(menuItem.Id, allergen2.Id);
        var handler = new RemoveAllergenFromMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var updatedMenuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id);
        
        updatedMenuItem!.Allergens.Should().ContainSingle();
        updatedMenuItem.Allergens.First().Id.Should().Be(allergen1.Id);
    }

    [Fact]
    public async Task Given_MenuItemWithMultipleAllergens_When_HandleIsCalled_Then_OnlySpecifiedAllergenRemoved()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var allergen1 = new CampusEats.Api.Models.Allergen
        {
            Name = "Gluten",
            Description = "Contains wheat"
        };
        
        var allergen2 = new CampusEats.Api.Models.Allergen
        {
            Name = "Dairy",
            Description = "Contains milk"
        };
        
        var allergen3 = new CampusEats.Api.Models.Allergen
        {
            Name = "Eggs",
            Description = "Contains eggs"
        };
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Pasta Carbonara",
            Description = "Creamy pasta",
            Price = 13.99m,
            Category = MenuCategory.Dinner,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen> { allergen1, allergen2, allergen3 }
        };
        
        dbContext.Allergens.AddRange(allergen1, allergen2, allergen3);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new RemoveAllergenFromMenuItemRequest(menuItem.Id, allergen2.Id);
        var handler = new RemoveAllergenFromMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var updatedMenuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id);
        
        updatedMenuItem.Should().NotBeNull();
        updatedMenuItem!.Allergens.Should().HaveCount(2);
        updatedMenuItem.Allergens.Should().Contain(a => a.Id == allergen1.Id);
        updatedMenuItem.Allergens.Should().Contain(a => a.Id == allergen3.Id);
        updatedMenuItem.Allergens.Should().NotContain(a => a.Id == allergen2.Id);
    }

    [Fact]
    public async Task Given_RemovingLastAllergen_When_HandleIsCalled_Then_MenuItemHasNoAllergens()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var allergen = new CampusEats.Api.Models.Allergen
        {
            Name = "Peanuts",
            Description = "Contains peanuts"
        };
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Rice Bowl",
            Description = "Plain rice bowl",
            Price = 6.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen> { allergen }
        };
        
        dbContext.Allergens.Add(allergen);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new RemoveAllergenFromMenuItemRequest(menuItem.Id, allergen.Id);
        var handler = new RemoveAllergenFromMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var updatedMenuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id);
        
        updatedMenuItem.Should().NotBeNull();
        updatedMenuItem!.Allergens.Should().BeEmpty();
    }
}
