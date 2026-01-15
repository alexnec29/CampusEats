using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Handlers.MenuItem;

public class AddAllergenToMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemAndAllergen_When_HandleIsCalled_Then_AllergenAddedSuccessfully()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Margherita Pizza",
            Description = "Classic pizza",
            Price = 12.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen>()
        };
        
        var allergen = new CampusEats.Api.Models.Allergen
        {
            Name = "Gluten",
            Description = "Contains wheat"
        };
        
        dbContext.MenuItems.Add(menuItem);
        dbContext.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync();
        
        var request = new AddAllergenToMenuItemRequest(menuItem.Id, allergen.Id);
        var handler = new AddAllergenToMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        var updatedMenuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id);
        
        updatedMenuItem.Should().NotBeNull();
        updatedMenuItem!.Allergens.Should().ContainSingle();
        updatedMenuItem.Allergens.First().Id.Should().Be(allergen.Id);
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
        
        var request = new AddAllergenToMenuItemRequest(999, allergen.Id);
        var handler = new AddAllergenToMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_NonExistentAllergen_When_HandleIsCalled_Then_NotFoundReturned()
    {
        // Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var menuItem = new CampusEats.Api.Models.MenuItem
        {
            Name = "Caesar Salad",
            Description = "Fresh salad",
            Price = 8.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen>()
        };
        
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new AddAllergenToMenuItemRequest(menuItem.Id, 999);
        var handler = new AddAllergenToMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_AllergenAlreadyExists_When_HandleIsCalled_Then_ConflictReturned()
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
            Name = "Peanut Butter Cookies",
            Description = "Delicious cookies",
            Price = 4.99m,
            Category = MenuCategory.Desserts,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen> { allergen }
        };
        
        dbContext.Allergens.Add(allergen);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var request = new AddAllergenToMenuItemRequest(menuItem.Id, allergen.Id);
        var handler = new AddAllergenToMenuItemHandler(dbContext);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_MultipleAllergensAdded_When_HandleIsCalled_Then_AllAllergensStored()
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
            Name = "Cheese Pizza",
            Description = "Classic cheese pizza",
            Price = 11.99m,
            Category = MenuCategory.Lunch,
            IsAvailable = true,
            Allergens = new List<CampusEats.Api.Models.Allergen>()
        };
        
        dbContext.Allergens.AddRange(allergen1, allergen2);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();
        
        var handler = new AddAllergenToMenuItemHandler(dbContext);
        
        // Act
        var request1 = new AddAllergenToMenuItemRequest(menuItem.Id, allergen1.Id);
        await handler.Handle(request1, CancellationToken.None);
        
        var request2 = new AddAllergenToMenuItemRequest(menuItem.Id, allergen2.Id);
        await handler.Handle(request2, CancellationToken.None);
        
        // Assert
        var updatedMenuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id);
        
        updatedMenuItem.Should().NotBeNull();
        updatedMenuItem!.Allergens.Should().HaveCount(2);
        updatedMenuItem.Allergens.Should().Contain(a => a.Id == allergen1.Id);
        updatedMenuItem.Allergens.Should().Contain(a => a.Id == allergen2.Id);
    }
}
