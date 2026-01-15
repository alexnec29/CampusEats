using CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace CampusEats.Test.Validators;

public class GetLoyaltyTransactionsValidatorTests
{
    [Fact]
    public async Task Given_EmptyUserId_When_Validated_Then_ValidationError()
    {
        //Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        var validator = new GetLoyaltyTransactionsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        GetLoyaltyTransactionsRequest request = new GetLoyaltyTransactionsRequest(
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
        var mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        
        mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Api.Models.User?)null);
        
        var validator = new GetLoyaltyTransactionsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        GetLoyaltyTransactionsRequest request = new GetLoyaltyTransactionsRequest(
            UserId: Guid.NewGuid()
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Loyalty account does not exist for this user.");
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
        
        var validator = new GetLoyaltyTransactionsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        GetLoyaltyTransactionsRequest request = new GetLoyaltyTransactionsRequest(
            UserId: userId
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Loyalty account does not exist for this user.");
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
        
        var validator = new GetLoyaltyTransactionsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        GetLoyaltyTransactionsRequest request = new GetLoyaltyTransactionsRequest(
            UserId: userId
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Loyalty account does not exist for this user.");
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
                PointsBalance = 100
            });
        
        var validator = new GetLoyaltyTransactionsValidator(
            mockUserRepository.Object,
            mockLoyaltyAccountRepository.Object
        );
        
        GetLoyaltyTransactionsRequest request = new GetLoyaltyTransactionsRequest(
            UserId: userId
        );
        
        //Act
        var result = await validator.TestValidateAsync(request);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
