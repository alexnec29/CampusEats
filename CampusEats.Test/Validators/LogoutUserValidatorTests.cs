using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class LogoutUserValidatorTests
{
    [Fact]
    public async Task Given_ValidToken_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new LogoutUserValidator();
        var request = new global::CampusEats.Api.Features.User.LogoutUserRequest("valid-jwt-token-string");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyToken_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new LogoutUserValidator();
        var request = new global::CampusEats.Api.Features.User.LogoutUserRequest("");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_NullToken_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new LogoutUserValidator();
        var request = new global::CampusEats.Api.Features.User.LogoutUserRequest(null);

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
