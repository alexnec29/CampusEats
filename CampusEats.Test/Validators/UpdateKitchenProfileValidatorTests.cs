using CampusEats.Api.Features.User;
using CampusEats.Api.Models;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class UpdateKitchenProfileValidatorTests
{
    private readonly UpdateKitchenProfileValidator _validator;

    public UpdateKitchenProfileValidatorTests()
    {
        AddressValidator addressValidator = new AddressValidator();
        WorkingHoursValidator workingHoursValidator = new WorkingHoursValidator();
        WeeklyWorkingHoursValidator weeklyWorkingHoursValidator = new WeeklyWorkingHoursValidator(workingHoursValidator);
        _validator = new UpdateKitchenProfileValidator(addressValidator, weeklyWorkingHoursValidator);
    }

    [Fact]
    public void Given_EmptyCompanyName_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateKitchenProfileRequest request = new UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "", // Empty company name
            new Address { street = "Main St", building = "10", city = "Cluj", county = "Cluj" },
            CreateValidWeeklyWorkingHours()
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }
    
    [Fact]
    public void Given_InvalidKitchenAddress_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateKitchenProfileRequest request = new UpdateKitchenProfileRequest(
            Guid.NewGuid(),
            "Test Kitchen",
            new Address { street = "", building = "10", city = "Cluj", county = "Cluj" }, // Empty street
            CreateValidWeeklyWorkingHours()
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.KitchenAddress.street)
            .WithErrorMessage("Street is required");
    }

    private WeeklyWorkingHours CreateValidWeeklyWorkingHours()
    {
        var workingHours = new WorkingHours 
        { 
            Open = TimeSpan.FromHours(9), 
            Close = TimeSpan.FromHours(18) 
        };
        
        return new WeeklyWorkingHours
        {
            Monday = workingHours,
            Tuesday = workingHours,
            Wednesday = workingHours,
            Thursday = workingHours,
            Friday = workingHours,
            Saturday = workingHours,
            Sunday = workingHours
        };
    }
}
