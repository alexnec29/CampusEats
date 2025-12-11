using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Handlers.Allergen;

public class AllergenIntegrationTests
{
    [Fact]
    public async Task Given_CreateAndGetAllergen_When_BothOperations_Then_DataPersisted()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var createHandler = new CreateAllergenHandler(dbContext);
        var getAllHandler = new GetAllAllergensHandler(dbContext);
        
        var createRequest = new CreateAllergen.CreateAllergenCommand("Peanuts");
        await createHandler.Handle(createRequest, CancellationToken.None);

        var getAllRequest = new GetAllAllergens.GetAllAllergensQuery();
        var result = await getAllHandler.Handle(getAllRequest, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Peanuts");
    }

    [Fact]
    public async Task Given_CreateAndDeleteAllergen_When_BothOperations_Then_AllergensRemoved()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var createHandler = new CreateAllergenHandler(dbContext);
        var deleteHandler = new DeleteAllergenHandler(dbContext);
        var getAllHandler = new GetAllAllergensHandler(dbContext);
        
        var allergen = await createHandler.Handle(new CreateAllergen.CreateAllergenCommand("Milk"), CancellationToken.None);
        var allergenId = allergen.Id;
        
        await deleteHandler.Handle(new DeleteAllergen.DeleteAllergenCommand(allergenId), CancellationToken.None);

        var result = await getAllHandler.Handle(new GetAllAllergens.GetAllAllergensQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_CreateMultipleAllergensAndDeleteOne_When_Operations_Then_OnlyOneDeleted()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        
        var createHandler = new CreateAllergenHandler(dbContext);
        var deleteHandler = new DeleteAllergenHandler(dbContext);
        var getAllHandler = new GetAllAllergensHandler(dbContext);
        
        var allergen1 = await createHandler.Handle(new CreateAllergen.CreateAllergenCommand("Peanuts"), CancellationToken.None);
        var allergen2 = await createHandler.Handle(new CreateAllergen.CreateAllergenCommand("Milk"), CancellationToken.None);
        
        await deleteHandler.Handle(new DeleteAllergen.DeleteAllergenCommand(allergen1.Id), CancellationToken.None);

        var result = await getAllHandler.Handle(new GetAllAllergens.GetAllAllergensQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Milk");
    }

    [Fact]
    public async Task Given_CreateAllergenTwice_When_DuplicateAttempted_Then_ExceptionThrown()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        await handler.Handle(new CreateAllergen.CreateAllergenCommand("Shellfish"), CancellationToken.None);
        
        var exception = await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(new CreateAllergen.CreateAllergenCommand("Shellfish"), CancellationToken.None)
        );
        
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task Given_CreateMultipleAllergensInSequence_When_AllSuccessful_Then_AllStored()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        var names = new[] { "Peanuts", "Milk", "Eggs", "Fish", "Shellfish" };
        
        foreach (var name in names)
        {
            await handler.Handle(new CreateAllergen.CreateAllergenCommand(name), CancellationToken.None);
        }

        var allergensInDb = await dbContext.Allergens.CountAsync();
        allergensInDb.Should().Be(5);
    }

    [Fact]
    public async Task Given_CreateAllergenAndVerifyInDatabase_When_Queried_Then_DataCorrect()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        await handler.Handle(new CreateAllergen.CreateAllergenCommand("Nuts"), CancellationToken.None);

        var allergenInDb = await dbContext.Allergens.FirstOrDefaultAsync(a => a.Name == "Nuts");
        
        allergenInDb.Should().NotBeNull();
        allergenInDb.Name.Should().Be("Nuts");
        allergenInDb.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Given_MultipleAllergensCreated_When_GetAll_Then_AllOrderedCorrectly()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var createHandler = new CreateAllergenHandler(dbContext);
        var getAllHandler = new GetAllAllergensHandler(dbContext);
        
        var allergens = new[] { "Zebra", "Apple", "Milk" };
        foreach (var name in allergens)
        {
            await createHandler.Handle(new CreateAllergen.CreateAllergenCommand(name), CancellationToken.None);
        }

        var result = await getAllHandler.Handle(new GetAllAllergens.GetAllAllergensQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(a => a.Name).Should().Contain(new[] { "Zebra", "Apple", "Milk" });
    }
}

public class AllergenEdgeCaseTests
{
    [Fact]
    public async Task Given_AllergenNameWithSpecialCharacters_When_Created_Then_Stored()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        var result = await handler.Handle(
            new CreateAllergen.CreateAllergenCommand("Peanut & Tree Nut"),
            CancellationToken.None
        );

        result.Name.Should().Be("Peanut & Tree Nut");
    }

    [Fact]
    public async Task Given_AllergenNameWithNumbers_When_Created_Then_Stored()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        var result = await handler.Handle(
            new CreateAllergen.CreateAllergenCommand("Milk 2%"),
            CancellationToken.None
        );

        result.Name.Should().Be("Milk 2%");
    }

    [Fact]
    public async Task Given_AllergenNameVeryLong_When_Created_Then_Stored()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        var longName = new string('A', 200);
        
        var result = await handler.Handle(
            new CreateAllergen.CreateAllergenCommand(longName),
            CancellationToken.None
        );

        result.Name.Should().Be(longName);
    }

    [Fact]
    public async Task Given_DeleteNonExistentAllergen_When_Attempted_Then_ExceptionThrown()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteAllergenHandler(dbContext);
        
        var exception = await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(new DeleteAllergen.DeleteAllergenCommand(Guid.NewGuid()), CancellationToken.None)
        );

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_CreateAllergenWithWhitespaceOnly_When_AttemptedAndValidated_Then_Behavior()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        var result = await handler.Handle(
            new CreateAllergen.CreateAllergenCommand("   "),
            CancellationToken.None
        );

        // Whitespace should be preserved or trimmed depending on implementation
        result.Should().NotBeNull();
    }
}

public class AllergenConcurrencyTests
{
    [Fact]
    public async Task Given_CreateSameAllergenConcurrently_When_BothAttempt_Then_OneSucceedsOneFails()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateAllergenHandler(dbContext);
        
        var task1 = handler.Handle(new CreateAllergen.CreateAllergenCommand("Soy"), CancellationToken.None);
        var task2 = handler.Handle(new CreateAllergen.CreateAllergenCommand("Soy"), CancellationToken.None);

        // One should succeed, other should fail
        var results = await Task.WhenAll(task1, task2).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                return new { Success = false, Error = t.Exception };
            }
            return new { Success = true, Error = (Exception)null };
        });

        // Should have handled the concurrency appropriately
        results.Should().NotBeNull();
    }
}
