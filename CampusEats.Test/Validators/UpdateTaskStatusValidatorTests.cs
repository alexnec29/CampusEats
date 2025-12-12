using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Validators;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class UpdateTaskStatusValidatorTests
{
    private readonly UpdateTaskStatusValidator _validator;

    public UpdateTaskStatusValidatorTests()
    {
        _validator = new UpdateTaskStatusValidator();
    }

    [Fact]
    public void Given_EmptyTaskId_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateTaskStatusCommand command = new UpdateTaskStatusCommand(0, "Pending");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskId);
    }
    
    [Fact]
    public void Given_InvalidStatus_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateTaskStatusCommand command = new UpdateTaskStatusCommand(1, "InvalidStatus");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.NewStatus)
            .WithErrorMessage("Invalid or unrecognized status value.");
    }
    
    [Fact]
    public void Given_ValidCommand_When_Validated_Then_NoValidationError()
    {
        //Arrange
        UpdateTaskStatusCommand command = new UpdateTaskStatusCommand(1, "Pending");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
