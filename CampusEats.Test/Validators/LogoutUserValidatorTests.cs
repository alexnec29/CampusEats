using CampusEats.Api.Features.User;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class LogoutUserValidatorTests
{
    private readonly LogoutUserValidator _validator;

    public LogoutUserValidatorTests()
    {
        _validator = new LogoutUserValidator();
    }

    [Fact]
    public void Given_EmptyJwt_When_Validated_Then_ValidationError()
    {
        //Arrange
        LogoutUserRequest request = new LogoutUserRequest(""); // Empty JWT
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Jwt);
    }
    
    [Fact]
    public void Given_ValidJwt_When_Validated_Then_NoValidationError()
    {
        //Arrange
        LogoutUserRequest request = new LogoutUserRequest("valid.jwt.token");
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
