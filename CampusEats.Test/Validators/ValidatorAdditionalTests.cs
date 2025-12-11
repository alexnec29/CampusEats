using CampusEats.Api.Validators;
using CampusEats.Api.Models;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class AddressValidatorTests
{
    [Fact]
    public async Task Given_ValidAddress_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        var result = await validator.ValidateAsync(address);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_MissingStreet_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        var result = await validator.ValidateAsync(address);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_MissingCity_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        var result = await validator.ValidateAsync(address);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_InvalidZipCode_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            ZipCode = "invalid",
            Country = "USA"
        };

        var result = await validator.ValidateAsync(address);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_VeryLongStreet_When_ValidateIsCalled_Then_ValidationChecked()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = new string('a', 256),
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        var result = await validator.ValidateAsync(address);

        // Should validate max length
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_AddressWithSpecialCharacters_When_ValidateIsCalled_Then_StillValid()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St. #5",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        var result = await validator.ValidateAsync(address);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AddressWithMultipleErrors_When_ValidateIsCalled_Then_MultipleErrorsReturned()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "",
            City = "",
            State = "",
            ZipCode = "",
            Country = ""
        };

        var result = await validator.ValidateAsync(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}

public class UpdateBuyerProfileValidatorTests
{
    [Fact]
    public async Task Given_ValidBuyerProfileUpdate_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        var validator = new UpdateBuyerProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "0712345678",
            "123 Street",
            "CreditCard"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_InvalidPhoneNumber_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new UpdateBuyerProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "invalid",
            "123 Street",
            "CreditCard"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_EmptyAddress_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new UpdateBuyerProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "0712345678",
            "",
            "CreditCard"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_InvalidPaymentMethod_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new UpdateBuyerProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "0712345678",
            "123 Street",
            "InvalidMethod"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }
}

public class UpdateKitchenProfileValidatorTests
{
    [Fact]
    public async Task Given_ValidKitchenProfileUpdate_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "Kitchen Name",
            "Description",
            "Cuisine Type"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyRestaurantName_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "",
            "Description",
            "Cuisine"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_EmptyDescription_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "Kitchen",
            "",
            "Cuisine"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_VeryLongRestaurantName_When_ValidateIsCalled_Then_ErrorReturned()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new global::CampusEats.Api.Features.User.UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            new string('a', 256),
            "Description",
            "Cuisine"
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }
}

public class WeeklyWorkingHoursValidatorTests
{
    [Fact]
    public async Task Given_ValidWeeklyHours_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        var validator = new WeeklyWorkingHoursValidator();
        var workingHours = new global::CampusEats.Api.Models.WeeklyWorkingHours
        {
            UserId = Guid.NewGuid()
        };

        var result = await validator.ValidateAsync(workingHours);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyUserId_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new WeeklyWorkingHoursValidator();
        var workingHours = new global::CampusEats.Api.Models.WeeklyWorkingHours
        {
            UserId = Guid.Empty
        };

        var result = await validator.ValidateAsync(workingHours);

        result.IsValid.Should().BeFalse();
    }
}

public class KitchenTaskValidatorTests
{
    [Fact]
    public async Task Given_ValidTaskRequest_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        var validator = new global::CampusEats.Api.Validators.KitchenTaskValidator();
        var request = new global::CampusEats.Api.Features.KitchenTask.CreateKitchenTaskRequest(
            "Task Title",
            "Task Description",
            null
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EmptyTaskTitle_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        var validator = new global::CampusEats.Api.Validators.KitchenTaskValidator();
        var request = new global::CampusEats.Api.Features.KitchenTask.CreateKitchenTaskRequest(
            "",
            "Description",
            null
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_TaskWithAssignee_When_ValidateIsCalled_Then_ValidIfValid()
    {
        var validator = new global::CampusEats.Api.Validators.KitchenTaskValidator();
        var request = new global::CampusEats.Api.Features.KitchenTask.CreateKitchenTaskRequest(
            "Task",
            "Description",
            Guid.NewGuid()
        );

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}
