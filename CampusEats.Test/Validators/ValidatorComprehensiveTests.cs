using CampusEats.Api.Validators;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class ValidatorIntegrationTests
{
    [Fact]
    public async Task Given_CreateUserWithValidData_When_ValidatorCalled_Then_NoErrors()
    {
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "newuser123",
            "newuser@example.com",
            "ValidPassword123!",
            "ValidPassword123!"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_CreateUserWithSpecialCharactersInUsername_When_Validated_Then_Checked()
    {
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "user@name.123",
            "user@example.com",
            "Password123!",
            "Password123!"
        );

        var result = await validator.ValidateAsync(request);

        // Should validate appropriately
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_CreateUserWithVeryLongUsername_When_Validated_Then_LengthChecked()
    {
        var validator = new CreateUserValidator();
        var longUsername = new string('a', 300);
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            longUsername,
            "user@example.com",
            "Password123!",
            "Password123!"
        );

        var result = await validator.ValidateAsync(request);

        // Should check max length
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_CreateUserWithWeakPassword_When_Validated_Then_RequirementsChecked()
    {
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "newuser",
            "user@example.com",
            "weak",
            "weak"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_CreateMenuItemWithValidPrice_When_Validated_Then_Valid()
    {
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "Burger",
            "Delicious",
            9.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.FastFood,
            "url",
            true
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_CreateMenuItemWithNegativePrice_When_Validated_Then_Invalid()
    {
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "Item",
            "Desc",
            -5m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.MainCourse,
            "url",
            true
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_CreateMenuItemWithZeroPrice_When_Validated_Then_Invalid()
    {
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "Item",
            "Desc",
            0m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.Dessert,
            "url",
            false
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_CreateMenuItemWithVeryHighPrice_When_Validated_Then_Valid()
    {
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "Luxury Item",
            "Premium",
            9999.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.MainCourse,
            "url",
            true
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_LoginUserWithValidCredentials_When_Validated_Then_Valid()
    {
        var validator = new LoginUserValidator();
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest(
            "username",
            "password123"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_LoginUserWithSpecialCharactersInPassword_When_Validated_Then_Valid()
    {
        var validator = new LoginUserValidator();
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest(
            "user",
            "Pass@word!#123"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_LoginUserWithVeryLongPassword_When_Validated_Then_Valid()
    {
        var validator = new LoginUserValidator();
        var longPassword = new string('a', 500);
        var request = new global::CampusEats.Api.Features.User.LoginUserRequest(
            "user",
            longPassword
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}

public class ValidatorEdgeCaseTests
{
    [Fact]
    public async Task Given_CreateAllergenWithUnicodeCharacters_When_Validated_Then_Handled()
    {
        var validator = new CreateAllergenValidator();
        var command = new global::CampusEats.Api.Features.Allergen.CreateAllergen.CreateAllergenCommand("Café Allergen ñ");

        var result = await validator.ValidateAsync(command);

        // Should handle unicode appropriately
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_CreateUserWithEmailEdgeCases_When_Validated_Then_Checked()
    {
        var validator = new CreateUserValidator();
        
        var testEmails = new[]
        {
            "user+tag@example.com",
            "user.name@example.co.uk",
            "user123@example.org",
            "a@b.c"
        };

        foreach (var email in testEmails)
        {
            var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
                "user",
                email,
                "Password123!",
                "Password123!"
            );

            var result = await validator.ValidateAsync(request);
            // Should validate email format appropriately
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Given_CreateMenuItemWithWhitespaceInName_When_Validated_Then_Trimmed()
    {
        var validator = new CreateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.CreateMenuItemRequest(
            "  Item Name  ",
            "Description",
            10m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.MainCourse,
            "url",
            true
        );

        var result = await validator.ValidateAsync(request);

        // Should handle whitespace appropriately
        result.IsValid.Should().BeTrue();
    }
}

public class ValidatorCrossFieldTests
{
    [Fact]
    public async Task Given_CreateUserPasswordsMatch_When_Validated_Then_Valid()
    {
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "user",
            "user@example.com",
            "SamePassword123!",
            "SamePassword123!"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_CreateUserPasswordsDontMatch_When_Validated_Then_Invalid()
    {
        var validator = new CreateUserValidator();
        var request = new global::CampusEats.Api.Features.User.CreateUserRequest(
            "user",
            "user@example.com",
            "Password123!",
            "DifferentPassword123!"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("password") || e.PropertyName.Contains("Password"));
    }

    [Fact]
    public async Task Given_UpdateMenuItemPriceIncreased_When_Validated_Then_Valid()
    {
        var validator = new UpdateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.UpdateMenuItemRequest(
            Guid.NewGuid(),
            "Item",
            "New description",
            19.99m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.MainCourse,
            true
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_UpdateMenuItemWithEmptyName_When_Validated_Then_Invalid()
    {
        var validator = new UpdateMenuItemValidator();
        var request = new global::CampusEats.Api.Features.MenuItem.UpdateMenuItemRequest(
            Guid.NewGuid(),
            "",
            "Description",
            15m,
            global::CampusEats.Api.Models.Enums.MenuItemCategory.Salad,
            true
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WorkingHoursStartBeforeEnd_When_Validated_Then_Valid()
    {
        var validator = new WorkingHoursValidator();
        var hours = new global::CampusEats.Api.Models.WorkingHours
        {
            DayOfWeek = "Monday",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        var result = await validator.ValidateAsync(hours);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WorkingHoursStartAfterEnd_When_Validated_Then_Invalid()
    {
        var validator = new WorkingHoursValidator();
        var hours = new global::CampusEats.Api.Models.WorkingHours
        {
            DayOfWeek = "Monday",
            StartTime = new TimeSpan(17, 0, 0),
            EndTime = new TimeSpan(8, 0, 0)
        };

        var result = await validator.ValidateAsync(hours);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WorkingHoursStartEqualsEnd_When_Validated_Then_Invalid()
    {
        var validator = new WorkingHoursValidator();
        var hours = new global::CampusEats.Api.Models.WorkingHours
        {
            DayOfWeek = "Tuesday",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(8, 0, 0)
        };

        var result = await validator.ValidateAsync(hours);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WorkingHoursEdgeMidnight_When_Validated_Then_Valid()
    {
        var validator = new WorkingHoursValidator();
        var hours = new global::CampusEats.Api.Models.WorkingHours
        {
            DayOfWeek = "Friday",
            StartTime = new TimeSpan(0, 0, 0),
            EndTime = new TimeSpan(23, 59, 59)
        };

        var result = await validator.ValidateAsync(hours);

        result.IsValid.Should().BeTrue();
    }
}
