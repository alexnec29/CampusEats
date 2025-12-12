using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class CreateMenuItemValidatorTests
{
    private readonly CreateMenuItemValidator _validator;

    public CreateMenuItemValidatorTests()
    {
        _validator = new CreateMenuItemValidator();
    }

    [Fact]
    public void Given_EmptyName_When_Validated_Then_ValidationError()
    {
        //Arrange
        CreateMenuItemRequest request = new CreateMenuItemRequest(
            "", // Empty name
            "Valid description",
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Given_NegativePrice_When_Validated_Then_ValidationError()
    {
        //Arrange
        CreateMenuItemRequest request = new CreateMenuItemRequest(
            "Pizza",
            "Delicious pizza",
            -5.99m, // Negative price
            MenuCategory.Lunch,
            null,
            true
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
    
    [Fact]
    public void Given_ValidMenuItem_When_Validated_Then_NoValidationError()
    {
        //Arrange
        CreateMenuItemRequest request = new CreateMenuItemRequest(
            "Pizza Margherita",
            "Classic Italian pizza with tomato sauce and mozzarella",
            25.99m,
            MenuCategory.Lunch,
            "https://example.com/pizza.jpg",
            true
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
