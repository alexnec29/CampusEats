using CampusEats.Api.Models;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class AddressValidatorTests
{
    private readonly AddressValidator _validator;

    public AddressValidatorTests()
    {
        _validator = new AddressValidator();
    }

    [Fact]
    public void Given_EmptyStreet_When_Validated_Then_ValidationError()
    {
        //Arrange
        Address address = new Address
        {
            street = "", // Empty street
            building = "10",
            city = "Cluj",
            county = "Cluj"
        };
        
        //Act
        var result = _validator.TestValidate(address);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.street)
            .WithErrorMessage("Street is required");
    }
    
    [Fact]
    public void Given_TooLongBuilding_When_Validated_Then_ValidationError()
    {
        //Arrange
        string longBuilding = new string('A', 101); // 101 characters
        Address address = new Address
        {
            street = "Main St",
            building = longBuilding,
            city = "Cluj",
            county = "Cluj"
        };
        
        //Act
        var result = _validator.TestValidate(address);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.building)
            .WithErrorMessage("Building is too long");
    }
    
    [Fact]
    public void Given_ValidAddress_When_Validated_Then_NoValidationError()
    {
        //Arrange
        Address address = new Address
        {
            street = "Main Street",
            building = "10A",
            city = "Cluj-Napoca",
            county = "Cluj"
        };
        
        //Act
        var result = _validator.TestValidate(address);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
