using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class CreateMenuItemValidatorTests
{
    [Fact]
    public async Task Given_ValidMenuItemRequest_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "Burger",
            "Delicious burger",
            9.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.FastFood,
            "https://example.com/image.jpg",
            true
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_MenuItemWithoutName_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "",
            "Description",
            9.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.MainCourse,
            "url",
            true
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_MenuItemWithNegativePrice_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "Item",
            "Description",
            -5.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.Salad,
            "url",
            true
        );

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
