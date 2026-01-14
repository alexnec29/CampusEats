using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Features.Allergen.DTOs;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CampusEats.Api.Features.Allergen.CreateAllergen;

namespace CampusEats.Test.Handlers.Allergen;

public class CreateAllergenHandlerTests
{
    [Fact]
    public async Task Given_ValidAllergenName_When_HandleIsCalled_Then_AllergenIsCreated()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        CreateAllergenCommand command = new CreateAllergenCommand("Peanuts");
        CreateAllergenHandler handler = new CreateAllergenHandler(dbContext);
        
        //Act
        AllergenResponse result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Peanuts");
        
        var savedAllergen = await dbContext.Allergens.FindAsync(result.Id);
        savedAllergen.Should().NotBeNull();
        savedAllergen!.Name.Should().Be("Peanuts");
    }
    
    [Fact]
    public async Task Given_DuplicateAllergenName_When_HandleIsCalled_Then_ExceptionIsThrown()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        // Add existing allergen
        dbContext.Allergens.Add(new Api.Models.Allergen { Name = "Gluten" });
        await dbContext.SaveChangesAsync();
        
        CreateAllergenCommand command = new CreateAllergenCommand("Gluten");
        CreateAllergenHandler handler = new CreateAllergenHandler(dbContext);
        
        //Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("already exists");
    }
    
    [Fact]
    public async Task Given_DuplicateAllergenNameWithDifferentCase_When_HandleIsCalled_Then_ExceptionIsThrown()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        // Add existing allergen
        dbContext.Allergens.Add(new Api.Models.Allergen { Name = "Dairy" });
        await dbContext.SaveChangesAsync();
        
        CreateAllergenCommand command = new CreateAllergenCommand("DAIRY");
        CreateAllergenHandler handler = new CreateAllergenHandler(dbContext);
        
        //Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("already exists");
    }
}
