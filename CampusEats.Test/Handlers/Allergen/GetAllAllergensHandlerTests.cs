using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Allergen;

public class GetAllAllergensHandlerTests
{
    [Fact]
    public async Task Given_NoAllergensInDatabase_When_HandleIsCalled_Then_EmptyListIsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetAllAllergensHandler(dbContext);
        var request = new GetAllAllergens.GetAllAllergensQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MultipleAllergensInDatabase_When_HandleIsCalled_Then_AllAllergensAreReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetAllAllergensHandler(dbContext);
        
        var allergens = new[]
        {
            new Api.Models.Allergen { Id = Guid.NewGuid(), Name = "Peanuts" },
            new Api.Models.Allergen { Id = Guid.NewGuid(), Name = "Milk" },
            new Api.Models.Allergen { Id = Guid.NewGuid(), Name = "Eggs" }
        };
        dbContext.Allergens.AddRange(allergens);
        await dbContext.SaveChangesAsync();

        var request = new GetAllAllergens.GetAllAllergensQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Select(a => a.Name).Should().Contain(new[] { "Peanuts", "Milk", "Eggs" });
    }

    [Fact]
    public async Task Given_AllergensInDatabase_When_HandleIsCalled_Then_CorrectDataIsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetAllAllergensHandler(dbContext);
        
        var allergenId = Guid.NewGuid();
        var allergen = new Api.Models.Allergen { Id = allergenId, Name = "Fish" };
        dbContext.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync();

        var request = new GetAllAllergens.GetAllAllergensQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Fish");
        result.First().Id.Should().Be(allergenId);
    }
}
