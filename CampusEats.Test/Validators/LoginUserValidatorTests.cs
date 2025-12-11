using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class LoginUserValidatorTests
{
    [Fact]
    public async Task Given_ValidLoginCredentials_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new LoginUserValidator();
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest("testuser", "password123");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyUsername_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new LoginUserValidator();
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest("", "password123");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_EmptyPassword_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new LoginUserValidator();
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest("testuser", "");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_BothEmptyUsernameAndPassword_When_ValidateIsCalled_Then_MultipleErrorsReturned()
    {
        // Arrange
        var validator = new LoginUserValidator();
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest("", "");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
