using CampusEats.Api.Features.Loyalty.RedeemPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace CampusEats.Test.Validators;

public class RedeemPointsValidatorTests
{
    [Fact]
    public async Task Given_ZeroPoints_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        var validator = new RedeemPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        RedeemPointsRequest request = new RedeemPointsRequest(
            UserId: Guid.NewGuid(),
            Points: 0,
            Description: "Redeem for discount"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Points)
            .WithErrorMessage("Points must be greater than zero.");
    }
    
    [Fact]
    public async Task Given_NegativePoints_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        var validator = new RedeemPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        RedeemPointsRequest request = new RedeemPointsRequest(
            UserId: Guid.NewGuid(),
            Points: -100,
            Description: "Redeem for order"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Points)
            .WithErrorMessage("Points must be greater than zero.");
    }
    
    [Fact]
    public async Task Given_NonExistentUser_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        
        mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Api.Models.User?)null);
        
        var validator = new RedeemPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        RedeemPointsRequest request = new RedeemPointsRequest(
            UserId: Guid.NewGuid(),
            Points: 100,
            Description: "Redeem rewards"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Invalid loyalty account or user.");
    }
    
    [Fact]
    public async Task Given_NonBuyerUser_When_Validated_Then_ValidationError()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        
        mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User 
            { 
                Id = userId, 
                Role = Role.Kitchen 
            });
        
        var validator = new RedeemPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        RedeemPointsRequest request = new RedeemPointsRequest(
            UserId: userId,
            Points: 100,
            Description: "Test"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Invalid loyalty account or user.");
    }
    
    [Fact]
    public async Task Given_BuyerWithNoLoyaltyAccount_When_Validated_Then_ValidationError()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        
        mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User 
            { 
                Id = userId, 
                Role = Role.Buyer 
            });
        
        mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Api.Models.LoyaltyAccount?)null);
        
        var validator = new RedeemPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        RedeemPointsRequest request = new RedeemPointsRequest(
            UserId: userId,
            Points: 100,
            Description: "Apply loyalty discount"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Invalid loyalty account or user.");
    }
    
    [Fact]
    public async Task Given_ValidBuyerWithLoyaltyAccount_When_Validated_Then_NoValidationError()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        
        mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User 
            { 
                Id = userId, 
                Role = Role.Buyer 
            });
        
        mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new Api.Models.LoyaltyAccount 
            { 
                Id = 1,
                UserId = userId,
                PointsBalance = 500
            });
        
        var validator = new RedeemPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        RedeemPointsRequest request = new RedeemPointsRequest(
            UserId: userId,
            Points: 100,
            Description: "Redeem for discount"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
