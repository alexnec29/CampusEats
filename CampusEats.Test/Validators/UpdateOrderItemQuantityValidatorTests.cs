using CampusEats.Api.Features.Order.UpdateOrderItemQuantity;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class UpdateOrderItemQuantityValidatorTests
{
    private readonly UpdateOrderItemQuantityValidator _validator;

    public UpdateOrderItemQuantityValidatorTests()
    {
        _validator = new UpdateOrderItemQuantityValidator();
    }

    [Fact]
    public void Given_ZeroQuantity_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(
            OrderId: 1,
            OrderItemId: 1,
            Quantity: 0
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }
    
    [Fact]
    public void Given_NegativeQuantity_When_Validated_Then_ValidationError()
    {
        //Arrange
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(
            OrderId: 1,
            OrderItemId: 1,
            Quantity: -5
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }
    
    [Fact]
    public void Given_ValidQuantity_When_Validated_Then_NoValidationError()
    {
        //Arrange
        UpdateOrderItemQuantityRequest request = new UpdateOrderItemQuantityRequest(
            OrderId: 1,
            OrderItemId: 1,
            Quantity: 5
        );
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
