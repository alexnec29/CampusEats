using CampusEats.Api.Validators;
using FluentValidation.TestHelper;
using static CampusEats.Api.Features.Allergen.CreateAllergen;

namespace CampusEats.Test.Validators;

public class CreateAllergenValidatorTests
{
    private readonly CreateAllergenValidator _validator;

    public CreateAllergenValidatorTests()
    {
        _validator = new CreateAllergenValidator();
    }

    [Fact]
    public void Given_EmptyName_When_Validated_Then_ValidationError()
    {
        //Arrange
        CreateAllergenCommand command = new CreateAllergenCommand("");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }
    
    [Fact]
    public void Given_TooLongName_When_Validated_Then_ValidationError()
    {
        //Arrange
        string longName = new string('A', 51); // 51 characters
        CreateAllergenCommand command = new CreateAllergenCommand(longName);
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 50 characters.");
    }
}
