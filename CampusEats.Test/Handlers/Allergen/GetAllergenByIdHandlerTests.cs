using CampusEats.Api.Features.Allergen.DTOs;
using CampusEats.Test.Helpers;
using FluentAssertions;
using static CampusEats.Api.Features.Allergen.GetAllergenById;

namespace CampusEats.Test.Handlers.Allergen;

public class GetAllergenByIdHandlerTests
{
    [Fact]
    public async Task Given_ExistingAllergenId_When_HandleIsCalled_Then_AllergenIsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var allergen = new Api.Models.Allergen { Name = "Peanuts" };
        dbContext.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync();

        GetAllergenByIdQuery query = new GetAllergenByIdQuery(allergen.Id);
        var handler = new Api.Features.Allergen.GetAllergenByIdHandler(dbContext);

        // Act
        AllergenResponse? result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(allergen.Id);
        result.Name.Should().Be("Peanuts");
    }

    [Fact]
    public async Task Given_NonExistentAllergenId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        GetAllergenByIdQuery query = new GetAllergenByIdQuery(999);
        var handler = new Api.Features.Allergen.GetAllergenByIdHandler(dbContext);

        // Act
        AllergenResponse? result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
