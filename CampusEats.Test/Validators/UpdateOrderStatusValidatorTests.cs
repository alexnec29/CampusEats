using CampusEats.Api.Features.Order.UpdateOrderStatus;
using CampusEats.Api.Models.Enums;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class UpdateOrderStatusValidatorTests
{
    private readonly UpdateOrderStatusValidator _validator;

    public UpdateOrderStatusValidatorTests()
    {
        _validator = new UpdateOrderStatusValidator();
    }

    [Fact]
    public void Given_ValidStatus_When_Validated_Then_NoValidationError()
    {
        //Arrange
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(
            OrderId: 1,
            Status: OrderStatus.Preparing
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Given_InvalidStatusValue_When_Validated_Then_ValidationError()
    {
        //Arrange
        // Using an invalid enum value (casting from int)
        UpdateOrderStatusRequest request = new UpdateOrderStatusRequest(
            OrderId: 1,
            Status: (OrderStatus)999
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Given_InactiveStatus_When_TransitionToPending_Then_TransitionAllowed()
    {
        //Act
        bool isAllowed = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Inactive, 
            OrderStatus.Pending
        );
        
        //Assert
        Assert.True(isAllowed);
    }

    [Fact]
    public void Given_PendingStatus_When_TransitionToPreparing_Then_TransitionAllowed()
    {
        //Act
        bool isAllowed = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Pending, 
            OrderStatus.Preparing
        );
        
        //Assert
        Assert.True(isAllowed);
    }

    [Fact]
    public void Given_PreparingStatus_When_TransitionToReady_Then_TransitionAllowed()
    {
        //Act
        bool isAllowed = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Preparing, 
            OrderStatus.Ready
        );
        
        //Assert
        Assert.True(isAllowed);
    }

    [Fact]
    public void Given_ReadyStatus_When_TransitionToCompleted_Then_TransitionAllowed()
    {
        //Act
        bool isAllowed = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Ready, 
            OrderStatus.Completed
        );
        
        //Assert
        Assert.True(isAllowed);
    }

    [Fact]
    public void Given_PendingStatus_When_TransitionToCompleted_Then_TransitionNotAllowed()
    {
        //Act
        bool isAllowed = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Pending, 
            OrderStatus.Completed
        );
        
        //Assert
        Assert.False(isAllowed);
    }

    [Fact]
    public void Given_CompletedStatus_When_TransitionToAnyStatus_Then_TransitionNotAllowed()
    {
        //Act
        bool isAllowedToPending = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Completed, 
            OrderStatus.Pending
        );
        bool isAllowedToCancelled = UpdateOrderStatusValidator.IsTransitionAllowed(
            OrderStatus.Completed, 
            OrderStatus.Cancelled
        );
        
        //Assert
        Assert.False(isAllowedToPending);
        Assert.False(isAllowedToCancelled);
    }
}
