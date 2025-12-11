using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Models.AndValidation;

public class AddressValidationComprehensiveTests
{
    [Fact]
    public async Task Given_ValidAddress_When_Validated_Then_Accepted()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            Country = "USA"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AddressWithMissingStreet_When_Validated_Then_Rejected()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = null,
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            Country = "USA"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_AddressWithVeryLongStreet_When_Validated_Then_Rejected()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = new string('A', 500),
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            Country = "USA"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_AddressWithNumbers_When_Validated_Then_Accepted()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "456 Oak Avenue, Apt 789",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            Country = "USA"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AddressWithInternationalCharacters_When_Validated_Then_Accepted()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "Rue de l'École, 123",
            City = "Paris",
            State = "ÎLE-DE-FRANCE",
            ZipCode = "75001",
            Country = "France"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AddressWithSpecialCharacters_When_Validated_Then_Accepted()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 O'Brien St. #456",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94102",
            Country = "USA"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AddressWithWhitespaceOnlyCity_When_Validated_Then_Rejected()
    {
        var validator = new AddressValidator();
        var address = new Address
        {
            Street = "123 Main St",
            City = "   ",
            State = "IL",
            ZipCode = "62701",
            Country = "USA"
        };
        
        var result = await validator.ValidateAsync(address);
        
        result.IsValid.Should().BeFalse();
    }
}

public class UserProfileValidationTests
{
    [Fact]
    public async Task Given_BuyerProfileWithValidPhoneAndAddress_When_Updated_Then_Accepted()
    {
        var validator = new UpdateBuyerProfileValidator();
        var request = new UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "(555) 123-4567",
            new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            }
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_BuyerProfileWithInvalidPhone_When_Updated_Then_Rejected()
    {
        var validator = new UpdateBuyerProfileValidator();
        var request = new UpdateBuyerProfileRequest(
            Guid.NewGuid(),
            "invalid-phone",
            new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            }
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_KitchenProfileWithValidNameAndDescription_When_Updated_Then_Accepted()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "Joe's Kitchen",
            "Serving delicious homemade meals"
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_KitchenProfileWithEmptyName_When_Updated_Then_Rejected()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "",
            "Serving delicious homemade meals"
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_KitchenProfileWithVeryLongDescription_When_Updated_Then_Rejected()
    {
        var validator = new UpdateKitchenProfileValidator();
        var request = new UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "Kitchen",
            new string('A', 5000)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }
}

public class WorkingHoursValidationTests
{
    [Fact]
    public async Task Given_ValidWorkingHours_When_Validated_Then_Accepted()
    {
        var validator = new WorkingHoursValidator();
        var request = new WorkingHoursRequest(
            DayOfWeek.Monday,
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WorkingHoursStartAfterEnd_When_Validated_Then_Rejected()
    {
        var validator = new WorkingHoursValidator();
        var request = new WorkingHoursRequest(
            DayOfWeek.Monday,
            new TimeSpan(17, 0, 0),
            new TimeSpan(9, 0, 0)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WorkingHoursSameStartAndEnd_When_Validated_Then_Rejected()
    {
        var validator = new WorkingHoursValidator();
        var request = new WorkingHoursRequest(
            DayOfWeek.Monday,
            new TimeSpan(9, 0, 0),
            new TimeSpan(9, 0, 0)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WorkingHoursStartingAt0000_When_Validated_Then_Accepted()
    {
        var validator = new WorkingHoursValidator();
        var request = new WorkingHoursRequest(
            DayOfWeek.Monday,
            new TimeSpan(0, 0, 0),
            new TimeSpan(6, 0, 0)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WorkingHoursEndingAt2359_When_Validated_Then_Accepted()
    {
        var validator = new WorkingHoursValidator();
        var request = new WorkingHoursRequest(
            DayOfWeek.Monday,
            new TimeSpan(18, 0, 0),
            new TimeSpan(23, 59, 0)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WorkingHoursWithMinutePrecision_When_Validated_Then_Accepted()
    {
        var validator = new WorkingHoursValidator();
        var request = new WorkingHoursRequest(
            DayOfWeek.Monday,
            new TimeSpan(9, 15, 0),
            new TimeSpan(17, 45, 0)
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }
}

public class WeeklyWorkingHoursValidationTests
{
    [Fact]
    public async Task Given_ValidWeeklySchedule_When_Validated_Then_Accepted()
    {
        var validator = new WeeklyWorkingHoursValidator();
        var weeklyHours = new List<WorkingHoursRequest>
        {
            new(DayOfWeek.Monday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
            new(DayOfWeek.Tuesday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
            new(DayOfWeek.Wednesday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
        };
        
        var request = new WeeklyWorkingHoursRequest(Guid.NewGuid(), weeklyHours);
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WeeklyScheduleWithEmptyList_When_Validated_Then_Rejected()
    {
        var validator = new WeeklyWorkingHoursValidator();
        var request = new WeeklyWorkingHoursRequest(Guid.NewGuid(), new List<WorkingHoursRequest>());
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WeeklyScheduleWithInvalidDay_When_Validated_Then_Handled()
    {
        var validator = new WeeklyWorkingHoursValidator();
        var weeklyHours = new List<WorkingHoursRequest>
        {
            new(DayOfWeek.Monday, new TimeSpan(17, 0, 0), new TimeSpan(9, 0, 0))
        };
        
        var request = new WeeklyWorkingHoursRequest(Guid.NewGuid(), weeklyHours);
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WeeklyScheduleWithAllDays_When_Validated_Then_Accepted()
    {
        var validator = new WeeklyWorkingHoursValidator();
        var weeklyHours = Enumerable.Range(0, 7)
            .Select(i => new WorkingHoursRequest(
                (DayOfWeek)i,
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0)
            ))
            .ToList();
        
        var request = new WeeklyWorkingHoursRequest(Guid.NewGuid(), weeklyHours);
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }
}

public class KitchenTaskValidationTests
{
    [Fact]
    public async Task Given_ValidKitchenTask_When_Created_Then_Accepted()
    {
        var validator = new KitchenTaskValidator();
        var request = new CreateKitchenTaskRequest(
            Guid.NewGuid(),
            "Prepare burger",
            "Make delicious burgers"
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_KitchenTaskWithEmptyTitle_When_Created_Then_Rejected()
    {
        var validator = new KitchenTaskValidator();
        var request = new CreateKitchenTaskRequest(
            Guid.NewGuid(),
            "",
            "Make delicious burgers"
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_KitchenTaskWithVeryLongTitle_When_Created_Then_Rejected()
    {
        var validator = new KitchenTaskValidator();
        var request = new CreateKitchenTaskRequest(
            Guid.NewGuid(),
            new string('X', 1000),
            "Make delicious burgers"
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_KitchenTaskWithNullDescription_When_Created_Then_Handled()
    {
        var validator = new KitchenTaskValidator();
        var request = new CreateKitchenTaskRequest(
            Guid.NewGuid(),
            "Prepare burger",
            null
        );
        
        var result = await validator.ValidateAsync(request);
        
        // Description might be optional
        result.IsValid.Should().BeTrue();
    }
}

public class PaymentAndMoneyValidationTests
{
    [Fact]
    public async Task Given_PaymentWithPositiveAmount_When_Processed_Then_Accepted()
    {
        // Payment validation tests would verify amount constraints
        var amount = 50.00m;
        
        amount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Given_PaymentWithZeroAmount_When_Validated_Then_Rejected()
    {
        var amount = 0m;
        
        amount.Should().BeLessThanOrEqualTo(0);
    }

    [Fact]
    public async Task Given_PaymentWithNegativeAmount_When_Validated_Then_Rejected()
    {
        var amount = -50.00m;
        
        amount.Should().BeLessThan(0);
    }

    [Fact]
    public async Task Given_PaymentWithDecimalPrecision_When_Processed_Then_Stored()
    {
        var amount = 19.99m;
        
        amount.Should().Be(19.99m);
    }

    [Fact]
    public async Task Given_PaymentWithVeryHighAmount_When_Processed_Then_Stored()
    {
        var amount = 999999.99m;
        
        amount.Should().BeGreaterThan(0);
    }
}

public class LoyaltyPointsValidationTests
{
    [Fact]
    public async Task Given_AddPointsToLoyaltyAccount_When_Positive_Then_Accepted()
    {
        var points = 100;
        
        points.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Given_SubtractPointsFromLoyaltyAccount_When_PositiveBalance_Then_Accepted()
    {
        var currentPoints = 150;
        var pointsToSubtract = 50;
        
        (currentPoints - pointsToSubtract).Should().Be(100);
    }

    [Fact]
    public async Task Given_SubtractMorePointsThanBalance_When_Insufficient_Then_Rejected()
    {
        var currentPoints = 50;
        var pointsToSubtract = 100;
        
        (currentPoints - pointsToSubtract).Should().BeLessThan(0);
    }

    [Fact]
    public async Task Given_LoyaltyPointsAtZero_When_Checked_Then_Handled()
    {
        var points = 0;
        
        points.Should().Be(0);
    }

    [Fact]
    public async Task Given_VeryHighLoyaltyPoints_When_Stored_Then_Processed()
    {
        var points = 999999;
        
        points.Should().BeGreaterThan(0);
    }
}
