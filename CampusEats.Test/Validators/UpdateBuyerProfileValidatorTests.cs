using CampusEats.Api.Features.User;
using CampusEats.Api.Models;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class UpdateBuyerProfileValidatorTests
{
    private readonly UpdateBuyerProfileValidator _validator;

    public UpdateBuyerProfileValidatorTests()
    {
        AddressValidator addressValidator = new AddressValidator();
        _validator = new UpdateBuyerProfileValidator(addressValidator);
    }

    [Fact]
    public void Given_EmptyFirstName_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateBuyerProfileRequest request = new UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "Doe",
            "", // Empty FirstName
            25,
            new Address { street = "Main St", building = "10", city = "Cluj", county = "Cluj" }
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }
    
    [Fact]
    public void Given_InvalidAge_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateBuyerProfileRequest request = new UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "Doe",
            "John",
            0, // Invalid age
            new Address { street = "Main St", building = "10", city = "Cluj", county = "Cluj" }
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Age)
            .WithErrorMessage("Age must be greater than 0");
    }
    
    [Fact]
    public void Given_InvalidAddress_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateBuyerProfileRequest request = new UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "Doe",
            "John",
            25,
            new Address { street = "", building = "10", city = "Cluj", county = "Cluj" } // Empty street
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress.street)
            .WithErrorMessage("Street is required");
    }
}
