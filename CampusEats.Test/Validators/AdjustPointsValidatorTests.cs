using CampusEats.Api.Features.Loyalty.AdjustPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace CampusEats.Test.Validators;

public class AdjustPointsValidatorTests
{
    [Fact]
    public async Task Given_ZeroPoints_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        var validator = new AdjustPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        AdjustPointsRequest request = new AdjustPointsRequest(
            UserId: Guid.NewGuid(),
            Points: 0,
            Reason: "Test"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Points)
            .WithErrorMessage("Points adjustment cannot be zero.");
    }
    
    [Fact]
    public async Task Given_NonExistentUser_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        
        mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Api.Models.User?)null);
        
        var validator = new AdjustPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        AdjustPointsRequest request = new AdjustPointsRequest(
            UserId: Guid.NewGuid(),
            Points: 100,
            Reason: "Test"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Invalid buyer or loyalty account.");
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
        
        var validator = new AdjustPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        AdjustPointsRequest request = new AdjustPointsRequest(
            UserId: userId,
            Points: 100,
            Reason: "Test"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Invalid buyer or loyalty account.");
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
        
        var validator = new AdjustPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        AdjustPointsRequest request = new AdjustPointsRequest(
            UserId: userId,
            Points: 100,
            Reason: "Test"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Invalid buyer or loyalty account.");
    }
    
    [Fact]
    public async Task Given_ValidBuyerWithLoyaltyAccount_PositivePoints_When_Validated_Then_NoValidationError()
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
        
        var validator = new AdjustPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        AdjustPointsRequest request = new AdjustPointsRequest(
            UserId: userId,
            Points: 100,
            Reason: "Bonus points"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Given_ValidBuyerWithLoyaltyAccount_NegativePoints_When_Validated_Then_NoValidationError()
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
        
        var validator = new AdjustPointsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        AdjustPointsRequest request = new AdjustPointsRequest(
            UserId: userId,
            Points: -50,
            Reason: "Correction"
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
