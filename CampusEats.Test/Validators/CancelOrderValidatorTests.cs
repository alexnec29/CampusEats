using CampusEats.Api.Features.Order.CancelOrder;
using FluentValidation.TestHelper;

namespace CampusEats.Test.Validators;

public class CancelOrderValidatorTests
{
    private readonly CancelOrderValidator _validator;

    public CancelOrderValidatorTests()
    {
        _validator = new CancelOrderValidator();
    }

    [Fact]
    public void Given_ZeroOrderId_When_Validated_Then_ValidationError()
    {
        //Arrange
        CancelOrderRequest request = new CancelOrderRequest(OrderId: 0);
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }
    
    [Fact]
    public void Given_NegativeOrderId_When_Validated_Then_ValidationError()
    {
        //Arrange
        CancelOrderRequest request = new CancelOrderRequest(OrderId: -5);
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }
    
    [Fact]
    public void Given_ValidOrderId_When_Validated_Then_NoValidationError()
    {
        //Arrange
        CancelOrderRequest request = new CancelOrderRequest(OrderId: 123);
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
