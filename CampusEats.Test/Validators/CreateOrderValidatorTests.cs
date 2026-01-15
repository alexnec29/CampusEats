using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Infrastructure.Repositories;
using FluentValidation.TestHelper;
using Moq;

namespace CampusEats.Test.Validators;

public class CreateOrderValidatorTests
{
    [Fact]
    public void Given_EmptyUserId_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var validator = new CreateOrderValidator(mockMenuItemRepository.Object);
        
        CreateOrderRequest request = new CreateOrderRequest(
            UserId: Guid.Empty,
            Notes: null
        );
        
        //Act
        var result = validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
    
    [Fact]
    public void Given_ValidUserId_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var validator = new CreateOrderValidator(mockMenuItemRepository.Object);
        
        CreateOrderRequest request = new CreateOrderRequest(
            UserId: Guid.NewGuid(),
            Notes: "Please deliver quickly"
        );
        
        //Act
        var result = validator.TestValidate(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
