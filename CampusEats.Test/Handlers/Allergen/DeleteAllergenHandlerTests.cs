using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Handlers.Allergen;

public class DeleteAllergenHandlerTests
{
    [Fact]
    public async Task Given_ValidAllergentId_When_HandleIsCalled_Then_AllergenIsDeleted()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteAllergenHandler(dbContext);
        
        var allergen = new Api.Models.Allergen { Id = Guid.NewGuid(), Name = "Peanuts" };
        dbContext.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync();

        var request = new DeleteAllergen.DeleteAllergenCommand(allergen.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedAllergen = await dbContext.Allergens.FirstOrDefaultAsync(a => a.Id == allergen.Id);
        deletedAllergen.Should().BeNull();
    }

    [Fact]
    public async Task Given_NonExistentAllergentId_When_HandleIsCalled_Then_ExceptionIsThrown()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteAllergenHandler(dbContext);
        var request = new DeleteAllergen.DeleteAllergenCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Given_ExistingAllergentId_When_HandleIsCalled_Then_OnlySpecificAllergenIsDeleted()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteAllergenHandler(dbContext);
        
        var allergen1 = new Api.Models.Allergen { Id = Guid.NewGuid(), Name = "Peanuts" };
        var allergen2 = new Api.Models.Allergen { Id = Guid.NewGuid(), Name = "Milk" };
        dbContext.Allergens.AddRange(allergen1, allergen2);
        await dbContext.SaveChangesAsync();

        var request = new DeleteAllergen.DeleteAllergenCommand(allergen1.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var remainingAllergensCount = await dbContext.Allergens.CountAsync();
        remainingAllergensCount.Should().Be(1);
        var remainingAllergen = await dbContext.Allergens.FirstOrDefaultAsync();
        remainingAllergen?.Id.Should().Be(allergen2.Id);
    }
}
