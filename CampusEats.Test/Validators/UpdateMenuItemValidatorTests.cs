using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class UpdateMenuItemValidatorTests
{
    private readonly UpdateMenuItemValidator _validator;

    public UpdateMenuItemValidatorTests()
    {
        _validator = new UpdateMenuItemValidator();
    }

    [Fact]
    public void Given_ValidRequest_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza Margherita",
            "Classic Italian pizza",
            12.99m,
            MenuCategory.Lunch,
            "https://example.com/pizza.jpg",
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Given_InvalidId_When_Validated_Then_ValidationError(int id)
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            id,
            "Pizza",
            "Delicious pizza",
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyOrWhitespaceName_When_Validated_Then_ValidationError(string name)
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            1,
            name,
            "Delicious pizza",
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Given_NameTooLong_When_Validated_Then_ValidationError()
    {
        //Arrange
        var longName = new string('A', 101); // 101 characters
        var request = new UpdateMenuItemRequest(
            1,
            longName,
            "Delicious pizza",
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyOrWhitespaceDescription_When_Validated_Then_ValidationError(string description)
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza",
            description,
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Given_DescriptionTooLong_When_Validated_Then_ValidationError()
    {
        //Arrange
        var longDescription = new string('A', 501); // 501 characters
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza",
            longDescription,
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-10.99)]
    public void Given_InvalidPrice_When_Validated_Then_ValidationError(decimal price)
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza",
            "Delicious pizza",
            price,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(1000.00)]
    public void Given_ValidPrice_When_Validated_Then_NoValidationError(decimal price)
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza",
            "Delicious pizza",
            price,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData(MenuCategory.Breakfast)]
    [InlineData(MenuCategory.Lunch)]
    [InlineData(MenuCategory.Dinner)]
    [InlineData(MenuCategory.Desserts)]
    [InlineData(MenuCategory.Drinks)]
    [InlineData(MenuCategory.Snacks)]
    public void Given_ValidCategory_When_Validated_Then_NoValidationError(MenuCategory category)
    {
        //Arrange
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza",
            "Delicious pizza",
            10.99m,
            category,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void Given_MaxLengthName_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var name = new string('A', 100); // Exactly 100 characters
        var request = new UpdateMenuItemRequest(
            1,
            name,
            "Delicious pizza",
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Given_MaxLengthDescription_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var description = new string('A', 500); // Exactly 500 characters
        var request = new UpdateMenuItemRequest(
            1,
            "Pizza",
            description,
            10.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
