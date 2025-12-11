using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Features.Allergen.DTOs;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Handlers.Allergen;

public class CreateAllergenHandlerTests
{
    [Fact]
    public async Task Given_ValidAllergenName_When_HandleIsCalled_Then_AllergenIsCreated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        var request = new CreateAllergen.CreateAllergenCommand("Peanuts");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Peanuts");
        var allergenInDb = await dbContext.Allergens.FirstOrDefaultAsync(a => a.Name == "Peanuts");
        allergenInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_DuplicateAllergenName_When_HandleIsCalled_Then_ExceptionIsThrown()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        var request1 = new CreateAllergen.CreateAllergenCommand("Milk");
        var request2 = new CreateAllergen.CreateAllergenCommand("Milk");

        // Act
        await handler.Handle(request1, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(request2, CancellationToken.None));

        // Assert
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task Given_DuplicateAllergenNameDifferentCase_When_HandleIsCalled_Then_ExceptionIsThrown()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        var request1 = new CreateAllergen.CreateAllergenCommand("Shellfish");
        var request2 = new CreateAllergen.CreateAllergenCommand("shellfish");

        // Act
        await handler.Handle(request1, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(request2, CancellationToken.None));

        // Assert
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task Given_MultipleAllergens_When_HandleIsCalled_Then_AllAreCreatedSuccessfully()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        var allergenNames = new[] { "Peanuts", "Milk", "Eggs", "Soy", "Fish" };

        // Act
        foreach (var name in allergenNames)
        {
            await handler.Handle(new CreateAllergen.CreateAllergenCommand(name), CancellationToken.None);
        }

        // Assert
        var allergensInDb = await dbContext.Allergens.ToListAsync();
        allergensInDb.Should().HaveCount(5);
    }
}
