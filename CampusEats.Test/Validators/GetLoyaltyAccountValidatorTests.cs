using CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace CampusEats.Test.Validators;

public class GetLoyaltyAccountValidatorTests
{
    [Fact]
    public async Task Given_EmptyUserId_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var validator = new GetLoyaltyAccountValidator(mockUserRepository.Object);
        
        GetLoyaltyAccountRequest request = new GetLoyaltyAccountRequest(
            UserId: Guid.Empty
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
    
    [Fact]
    public async Task Given_NonExistentUser_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Api.Models.User?)null);
        
        var validator = new GetLoyaltyAccountValidator(mockUserRepository.Object);
        
        GetLoyaltyAccountRequest request = new GetLoyaltyAccountRequest(
            UserId: Guid.NewGuid()
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Loyalty accounts are only available for buyers.");
    }
    
    [Fact]
    public async Task Given_NonBuyerUser_When_Validated_Then_ValidationError()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User 
            { 
                Id = userId, 
                Role = Role.Kitchen 
            });
        
        var validator = new GetLoyaltyAccountValidator(mockUserRepository.Object);
        
        GetLoyaltyAccountRequest request = new GetLoyaltyAccountRequest(
            UserId: userId
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Loyalty accounts are only available for buyers.");
    }
    
    [Fact]
    public async Task Given_ValidBuyerUser_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User 
            { 
                Id = userId, 
                Role = Role.Buyer 
            });
        
        var validator = new GetLoyaltyAccountValidator(mockUserRepository.Object);
        
        GetLoyaltyAccountRequest request = new GetLoyaltyAccountRequest(
            UserId: userId
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
