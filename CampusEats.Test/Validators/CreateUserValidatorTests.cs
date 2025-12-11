using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class CreateUserValidatorTests
{
    [Fact]
    public async Task Given_ValidUserRequest_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "newuser",
            "user@example.com",
            "ValidPassword123!",
            "ValidPassword123!"
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyUsername_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "",
            "user@example.com",
            "ValidPassword123!",
            "ValidPassword123!"
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_InvalidEmailFormat_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "newuser",
            "notanemail",
            "ValidPassword123!",
            "ValidPassword123!"
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_PasswordsDoNotMatch_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "newuser",
            "user@example.com",
            "ValidPassword123!",
            "DifferentPassword123!"
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
