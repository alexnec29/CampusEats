using CampusEats.Api.Models;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class WeeklyWorkingHoursValidatorTests
{
    private readonly WeeklyWorkingHoursValidator _validator;

    public WeeklyWorkingHoursValidatorTests()
    {
        WorkingHoursValidator workingHoursValidator = new WorkingHoursValidator();
        _validator = new WeeklyWorkingHoursValidator(workingHoursValidator);
    }

    [Fact]
    public void Given_InvalidMondayHours_When_Validated_Then_ValidationError()
    {
        //Arrange
        WeeklyWorkingHours weeklyHours = new WeeklyWorkingHours
        {
            Monday = new WorkingHours { Open = TimeSpan.FromHours(26), Close = TimeSpan.FromHours(18) }, // Invalid
            Tuesday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Wednesday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Thursday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Friday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Saturday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Sunday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) }
        };
        
        //Act
        var result = _validator.TestValidate(weeklyHours);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Monday.Open);
    }
    
    [Fact]
    public void Given_ValidWeeklyHours_When_Validated_Then_NoValidationError()
    {
        //Arrange
        WeeklyWorkingHours weeklyHours = new WeeklyWorkingHours
        {
            Monday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Tuesday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Wednesday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Thursday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Friday = new WorkingHours { Open = TimeSpan.FromHours(9), Close = TimeSpan.FromHours(18) },
            Saturday = new WorkingHours { Open = TimeSpan.FromHours(10), Close = TimeSpan.FromHours(16) },
            Sunday = new WorkingHours { Open = TimeSpan.FromHours(10), Close = TimeSpan.FromHours(16) }
        };
        
        //Act
        var result = _validator.TestValidate(weeklyHours);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
