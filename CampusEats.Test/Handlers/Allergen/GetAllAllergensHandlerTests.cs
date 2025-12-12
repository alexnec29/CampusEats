using CampusEats.Api.Features.Allergen.DTOs;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;
using static CampusEats.Api.Features.Allergen.GetAllAllergens;

namespace CampusEats.Test.Handlers.Allergen;

public class GetAllAllergensHandlerTests
{
    [Fact]
    public async Task Given_AllergensExist_When_HandleIsCalled_Then_AllAllergensAreReturned()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        dbContext.Allergens.Add(new Api.Models.Allergen { Name = "Peanuts" });
        dbContext.Allergens.Add(new Api.Models.Allergen { Name = "Gluten" });
        dbContext.Allergens.Add(new Api.Models.Allergen { Name = "Dairy" });
        await dbContext.SaveChangesAsync();
        
        GetAllAllergensQuery query = new GetAllAllergensQuery();
        var handler = new Api.Features.Allergen.GetAllAllergensHandler(dbContext);
        
        //Act
        IEnumerable<AllergenResponse> result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Name == "Peanuts");
        result.Should().Contain(a => a.Name == "Gluten");
        result.Should().Contain(a => a.Name == "Dairy");
    }
    
    [Fact]
    public async Task Given_NoAllergensExist_When_HandleIsCalled_Then_EmptyListIsReturned()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        GetAllAllergensQuery query = new GetAllAllergensQuery();
        var handler = new Api.Features.Allergen.GetAllAllergensHandler(dbContext);
        
        //Act
        IEnumerable<AllergenResponse> result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
