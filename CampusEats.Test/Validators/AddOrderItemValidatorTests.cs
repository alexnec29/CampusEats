using CampusEats.Api.Features.Order.AddOrderItem;
using CampusEats.Api.Infrastructure.Repositories;
using FluentValidation.TestHelper;
using Moq;

namespace CampusEats.Test.Validators;

public class AddOrderItemValidatorTests
{
    [Fact]
    public async Task Given_EmptyOrderId_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var validator = new AddOrderItemValidator(mockMenuItemRepository.Object);
        
        AddOrderItemRequest request = new AddOrderItemRequest(
            OrderId: 0,
            MenuItemId: 1,
            Quantity: 1
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }
    
    [Fact]
    public async Task Given_EmptyMenuItemId_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var validator = new AddOrderItemValidator(mockMenuItemRepository.Object);
        
        AddOrderItemRequest request = new AddOrderItemRequest(
            OrderId: 1,
            MenuItemId: 0,
            Quantity: 1
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.MenuItemId);
    }
    
    [Fact]
    public async Task Given_ZeroQuantity_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var validator = new AddOrderItemValidator(mockMenuItemRepository.Object);
        
        AddOrderItemRequest request = new AddOrderItemRequest(
            OrderId: 1,
            MenuItemId: 1,
            Quantity: 0
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }
    
    [Fact]
    public async Task Given_NegativeQuantity_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var validator = new AddOrderItemValidator(mockMenuItemRepository.Object);
        
        AddOrderItemRequest request = new AddOrderItemRequest(
            OrderId: 1,
            MenuItemId: 1,
            Quantity: -5
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }
    
    [Fact]
    public async Task Given_NonExistentMenuItem_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        mockMenuItemRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Api.Models.MenuItem?)null);
        
        var validator = new AddOrderItemValidator(mockMenuItemRepository.Object);
        
        AddOrderItemRequest request = new AddOrderItemRequest(
            OrderId: 1,
            MenuItemId: 999,
            Quantity: 1
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.MenuItemId)
            .WithErrorMessage("Menu item does not exist.");
    }
    
    [Fact]
    public async Task Given_ValidRequest_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        mockMenuItemRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Api.Models.MenuItem { Id = 1, Name = "Test Item" });
        
        var validator = new AddOrderItemValidator(mockMenuItemRepository.Object);
        
        AddOrderItemRequest request = new AddOrderItemRequest(
            OrderId: 1,
            MenuItemId: 1,
            Quantity: 2
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
