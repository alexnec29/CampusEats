using CampusEats.Api.Features.User;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class LoginUserValidatorTests
{
    private readonly LoginUserValidator _validator;

    public LoginUserValidatorTests()
    {
        _validator = new LoginUserValidator();
    }

    [Fact]
    public void Given_EmptyUsername_When_Validated_Then_ValidationError()
    {
        //Arrange
        LoginUserRequest request = new LoginUserRequest("", "ValidPassword123!");
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required");
    }
    
    [Fact]
    public void Given_InvalidPassword_When_Validated_Then_ValidationError()
    {
        //Arrange
        LoginUserRequest request = new LoginUserRequest("validUsername", "short");
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }
}
