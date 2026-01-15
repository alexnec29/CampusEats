using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class AllergenRepositoryTests
{
    [Fact]
    public async Task Given_ValidAllergen_When_AddAsyncCalled_Then_AllergenAdded()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new AllergenRepository(dbContext);
        var allergen = new Allergen
        {
            Name = "Peanuts",
            Description = "All types of peanuts"
        };

        await repository.AddAsync(allergen);

        var savedAllergen = await repository.GetByIdAsync(allergen.Id);
        savedAllergen.Should().NotBeNull();
        savedAllergen!.Name.Should().Be("Peanuts");
    }

    [Fact]
    public async Task Given_ExistingAllergen_When_UpdateAsyncCalled_Then_AllergenUpdated()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new AllergenRepository(dbContext);
        var allergen = new Allergen { Name = "Milk", Description = "Dairy" };
        await repository.AddAsync(allergen);

        allergen.Description = "Dairy products";
        await repository.UpdateAsync(allergen);

        var updated = await repository.GetByIdAsync(allergen.Id);
        updated!.Description.Should().Be("Dairy products");
    }

    [Fact]
    public async Task Given_ExistingAllergen_When_DeleteAsyncCalled_Then_AllergenDeleted()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new AllergenRepository(dbContext);
        var allergen = new Allergen { Name = "Soy", Description = "Soybeans" };
        await repository.AddAsync(allergen);
        var id = allergen.Id;

        await repository.DeleteAsync(id);

        var deleted = await repository.GetByIdAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Given_MultipleAllergens_When_GetAllAsyncCalled_Then_ReturnsAll()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new AllergenRepository(dbContext);
        await repository.AddAsync(new Allergen { Name = "A1" });
        await repository.AddAsync(new Allergen { Name = "A2" });

        var result = await repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_MenuItemWithAllergens_When_GetAllergensForMenuItemAsyncCalled_Then_ReturnsLinkedAllergens()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new AllergenRepository(dbContext);

        var allergen1 = new Allergen { Name = "Gluten" };
        var allergen2 = new Allergen { Name = "Eggs" };
        var menuItem = new MenuItem { Name = "Pasta", Price = 10 };
        
        dbContext.Allergens.AddRange(allergen1, allergen2);
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();

        dbContext.MenuItemAllergens.Add(new MenuItemAllergen 
        { 
            MenuItemId = menuItem.Id, 
            AllergenId = allergen1.Id,
            Allergen = allergen1 
        });
        await dbContext.SaveChangesAsync();

        var result = await repository.GetAllergensForMenuItemAsync(menuItem.Id);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Gluten");
    }
}