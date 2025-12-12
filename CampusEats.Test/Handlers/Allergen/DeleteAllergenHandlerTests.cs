using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;
using MediatR;
using static CampusEats.Api.Features.Allergen.DeleteAllergen;

namespace CampusEats.Test.Handlers.Allergen;

public class DeleteAllergenHandlerTests
{
    [Fact]
    public async Task Given_ExistingAllergenId_When_HandleIsCalled_Then_AllergenIsDeleted()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var allergen = new Api.Models.Allergen { Name = "Soy" };
        dbContext.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync();
        
        DeleteAllergenCommand command = new DeleteAllergenCommand(allergen.Id);
        var handler = new Api.Features.Allergen.DeleteAllergenHandler(dbContext);
        
        //Act
        Unit result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        result.Should().Be(Unit.Value);
        
        var deletedAllergen = await dbContext.Allergens.FindAsync(allergen.Id);
        deletedAllergen.Should().BeNull();
    }
    
    [Fact]
    public async Task Given_NonExistentAllergenId_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        //Arrange
        CampusEatsDbContext dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        DeleteAllergenCommand command = new DeleteAllergenCommand(999);
        var handler = new Api.Features.Allergen.DeleteAllergenHandler(dbContext);
        
        //Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("not found");
        exception.Message.Should().Contain("999");
    }
}
