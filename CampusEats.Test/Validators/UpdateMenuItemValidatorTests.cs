using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class UpdateMenuItemValidatorTests
{
    [Fact]
    public async Task Given_ValidUpdateRequest_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new UpdateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.UpdateMenuItemRequest(
            Guid.NewGuid(),
            "Updated Burger",
            "Updated description",
            12.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.MainCourse,
            true
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyName_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new UpdateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.UpdateMenuItemRequest(
            Guid.NewGuid(),
            "",
            "Description",
            9.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.FastFood,
            true
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_InvalidPrice_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new UpdateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.UpdateMenuItemRequest(
            Guid.NewGuid(),
            "Item",
            "Description",
            0,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.Dessert,
            false
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
