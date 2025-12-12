using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class AssignTaskToStaffValidatorTests
{
    private readonly AssignTaskToStaffValidator _validator;

    public AssignTaskToStaffValidatorTests()
    {
        _validator = new AssignTaskToStaffValidator();
    }

    [Fact]
    public void Given_EmptyTaskId_When_Validated_Then_ValidationError()
    {
        //Arrange
        AssignTaskToStaffCommand command = new AssignTaskToStaffCommand(0, Guid.NewGuid());
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskId);
    }
    
    [Fact]
    public void Given_EmptyStaffId_When_Validated_Then_ValidationError()
    {
        //Arrange
        AssignTaskToStaffCommand command = new AssignTaskToStaffCommand(1, Guid.Empty);
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.StaffId);
    }
    
    [Fact]
    public void Given_ValidCommand_When_Validated_Then_NoValidationError()
    {
        //Arrange
        AssignTaskToStaffCommand command = new AssignTaskToStaffCommand(1, Guid.NewGuid());
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
