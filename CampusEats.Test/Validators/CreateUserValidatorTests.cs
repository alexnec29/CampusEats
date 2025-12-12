using CampusEats.Api.Features.User;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator;

    public CreateUserValidatorTests()
    {
        _validator = new CreateUserValidator();
    }

    [Fact]
    public void Given_EmptyEmail_When_Validated_Then_ValidationError()
    {
        //Arrange
        CreateUserRequest request = new CreateUserRequest(
            "validUsername",
            "", // Empty email
            "ValidPassword123!",
            "ValidPassword123!"
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }
    
    [Fact]
    public void Given_InvalidEmailFormat_When_Validated_Then_ValidationError()
    {
        //Arrange
        CreateUserRequest request = new CreateUserRequest(
            "validUsername",
            "notAnEmail", // Invalid email format
            "ValidPassword123!",
            "ValidPassword123!"
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }
    
    [Fact]
    public void Given_PasswordWithoutSpecialCharacter_When_Validated_Then_ValidationError()
    {
        //Arrange
        CreateUserRequest request = new CreateUserRequest(
            "validUsername",
            "valid@email.com",
            "ValidPassword123", // Missing special character
            "ValidPassword123"
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one special character");
    }
}
