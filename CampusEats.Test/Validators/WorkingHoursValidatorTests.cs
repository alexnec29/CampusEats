using CampusEats.Api.Validators;
using CampusEats.Api.Models;
using FluentAssertions;

namespace CampusEats.Test.Validators;

public class WorkingHoursValidatorTests
{
    [Fact]
    public async Task Given_ValidWorkingHours_When_ValidateIsCalled_Then_NoErrorsReturned()
    {
        // Arrange
        var validator = new WorkingHoursValidator();
        var workingHours = new WorkingHours
        {
            DayOfWeek = "Monday",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        // Act
        var result = await validator.ValidateAsync(workingHours);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Given_EndTimeBeforeStartTime_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new WorkingHoursValidator();
        var workingHours = new WorkingHours
        {
            DayOfWeek = "Monday",
            StartTime = new TimeSpan(17, 0, 0),
            EndTime = new TimeSpan(8, 0, 0)
        };

        // Act
        var result = await validator.ValidateAsync(workingHours);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_SameStartAndEndTime_When_ValidateIsCalled_Then_ErrorIsReturned()
    {
        // Arrange
        var validator = new WorkingHoursValidator();
        var workingHours = new WorkingHours
        {
            DayOfWeek = "Monday",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(8, 0, 0)
        };

        // Act
        var result = await validator.ValidateAsync(workingHours);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
