using CampusEats.Api.Validators;
using CampusEats.Api.Models;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class AddressValidatorTests
{
    [Fact]
    public async Task Given_ValidAddress_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "New York",
            Country = "USA",
            PostalCode = "10001"
        };

        // Act
        var result = await validator.ValidateAsync(address);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AddressWithoutStreet_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "",
            City = "New York",
            Country = "USA",
            PostalCode = "10001"
        };

        // Act
        var result = await validator.ValidateAsync(address);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_AddressWithoutCity_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "",
            Country = "USA",
            PostalCode = "10001"
        };

        // Act
        var result = await validator.ValidateAsync(address);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
