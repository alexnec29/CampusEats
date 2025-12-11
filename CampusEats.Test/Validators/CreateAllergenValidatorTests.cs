using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class CreateAllergenValidatorTests
{
    [Fact]
    public async Task Given_ValidAllergenRequest_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new CreateAllergenValidator();
        var command = new global::CampusEats.Api.Features.Allergen.CreateAllergen.CreateAllergenCommand("Peanuts");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyAllergenName_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new CreateAllergenValidator();
        var command = new global::CampusEats.Api.Features.Allergen.CreateAllergen.CreateAllergenCommand("");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
}
