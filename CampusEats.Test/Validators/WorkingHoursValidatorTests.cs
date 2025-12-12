using CampusEats.Api.Models;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class WorkingHoursValidatorTests
{
    private readonly WorkingHoursValidator _validator;

    public WorkingHoursValidatorTests()
    {
        _validator = new WorkingHoursValidator();
    }

    [Fact]
    public void Given_InvalidOpenTime_When_Validated_Then_ValidationError()
    {
        //Arrange
        WorkingHours workingHours = new WorkingHours
        {
            Open = TimeSpan.FromHours(25), // Invalid - over 24 hours
            Close = TimeSpan.FromHours(18)
        };
        
        //Act
        var result = _validator.TestValidate(workingHours);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Open)
            .WithErrorMessage("Open must be between 00:00 and 23:59:59");
    }
    
    [Fact]
    public void Given_ValidWorkingHours_When_Validated_Then_NoValidationError()
    {
        //Arrange
        WorkingHours workingHours = new WorkingHours
        {
            Open = TimeSpan.FromHours(9),   // 09:00
            Close = TimeSpan.FromHours(18)  // 18:00
        };
        
        //Act
        var result = _validator.TestValidate(workingHours);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
