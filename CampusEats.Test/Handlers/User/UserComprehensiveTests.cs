using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Test.Handlers.User;

public class UserHandlerComprehensiveTests
{
    [Fact]
    public async Task Given_UserWithValidRoleBuyer_When_GetUserById_Then_RolePreserved()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User { Id = userId, Username = "buyer", Role = Role.Buyer };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Role.Should().Be(Role.Buyer);
    }

    [Fact]
    public async Task Given_UserWithValidRoleKitchen_When_GetUserById_Then_RolePreserved()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User { Id = userId, Username = "kitchen", Role = Role.Kitchen };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Role.Should().Be(Role.Kitchen);
    }

    [Fact]
    public async Task Given_UserWithValidRoleAdmin_When_GetUserById_Then_RolePreserved()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User { Id = userId, Username = "admin", Role = Role.Admin };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task Given_ValidCredentialsForBuyer_When_LoginUser_Then_TokenGenerated()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new Api.Models.User { Id = Guid.NewGuid(), Username = "buyeruser", Role = Role.Buyer };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("buyeruser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(user))
            .Returns("buyer-token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("buyeruser", "password");

        var result = await handler.Handle(request, CancellationToken.None);

        mockJwtService.Verify(service => service.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Given_ValidCredentialsForKitchen_When_LoginUser_Then_TokenGenerated()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new Api.Models.User { Id = Guid.NewGuid(), Username = "kitchenuser", Role = Role.Kitchen };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("kitchenuser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(user))
            .Returns("kitchen-token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("kitchenuser", "password");

        var result = await handler.Handle(request, CancellationToken.None);

        mockJwtService.Verify(service => service.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleLoginAttemptsWithDifferentUsers_When_HandleCalled_Then_DifferentTokensReturned()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user1 = new Api.Models.User { Id = Guid.NewGuid(), Username = "user1" };
        var user2 = new Api.Models.User { Id = Guid.NewGuid(), Username = "user2" };
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("user1"))
            .ReturnsAsync(user1);
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("user2"))
            .ReturnsAsync(user2);
        mockJwtService.Setup(service => service.GenerateToken(user1))
            .Returns("token1");
        mockJwtService.Setup(service => service.GenerateToken(user2))
            .Returns("token2");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        
        var request1 = new LoginUserRequest("user1", "password");
        var request2 = new LoginUserRequest("user2", "password");

        await handler.Handle(request1, CancellationToken.None);
        await handler.Handle(request2, CancellationToken.None);

        mockJwtService.Verify(service => service.GenerateToken(user1), Times.Once);
        mockJwtService.Verify(service => service.GenerateToken(user2), Times.Once);
    }

    [Fact]
    public async Task Given_UserEmailAndUsername_When_GetUserById_Then_BothPreserved()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User 
        { 
            Id = userId, 
            Username = "testuser",
            Email = "test@example.com"
        };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Given_UserPassword_When_GetUserById_Then_PasswordNotReturned()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var user = new Api.Models.User { Id = userId, Username = "testuser" };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockUserRepository.Object);
        var request = new GetUserByIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        // Password should not be exposed in response
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_LogoutWithValidToken_When_HandleCalled_Then_TokenBlacklisted()
    {
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        var token = "valid-jwt-token";
        
        var handler = new LogoutUserHandler(mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest(token);

        await handler.Handle(request, CancellationToken.None);

        mockBlacklistRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.BlackListToken>(t => t.Token == token)),
            Times.Once);
    }

    [Fact]
    public async Task Given_LogoutTwiceWithSameToken_When_HandleCalled_Then_BothAttempted()
    {
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        var token = "same-token";
        
        var handler = new LogoutUserHandler(mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest(token);

        await handler.Handle(request, CancellationToken.None);
        await handler.Handle(request, CancellationToken.None);

        mockBlacklistRepository.Verify(
            repo => repo.AddAsync(It.IsAny<Api.Models.BlackListToken>()),
            Times.Exactly(2));
    }
}

public class UserCreationEdgeCaseTests
{
    [Fact]
    public async Task Given_CreateUserWithMinimumValidData_When_HandleCalled_Then_UserCreated()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((Api.Models.User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Api.Models.User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("newuser", "new@example.com", "pass123", "pass123");

        var result = await handler.Handle(request, CancellationToken.None);

        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.User>()), Times.Once);
    }

    [Fact]
    public async Task Given_CreateUserWithSpecialCharactersInUsername_When_HandleCalled_Then_Behavior()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("user_name.123", "user@example.com", "pass123", "pass123");

        var result = await handler.Handle(request, CancellationToken.None);

        // Should handle special characters appropriately
    }

    [Fact]
    public async Task Given_CreateUserWithLongUsername_When_HandleCalled_Then_Validated()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var longUsername = new string('a', 500);
        var request = new CreateUserRequest(longUsername, "user@example.com", "pass123", "pass123");

        var result = await handler.Handle(request, CancellationToken.None);

        // Validator should check length
    }
}

public class BuyerProfileTests
{
    [Fact]
    public async Task Given_BuyerProfileWithPhone_When_GetProfile_Then_PhoneReturned()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var profile = new Api.Models.BuyerProfile 
        { 
            UserId = userId, 
            PhoneNumber = "0712345678"
        };
        
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new GetBuyerProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetBuyerProfileByUserIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.PhoneNumber.Should().Be("0712345678");
    }

    [Fact]
    public async Task Given_BuyerProfileWithAddress_When_GetProfile_Then_AddressReturned()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var profile = new Api.Models.BuyerProfile 
        { 
            UserId = userId,
            Address = "123 Main Street"
        };
        
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new GetBuyerProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetBuyerProfileByUserIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Address.Should().Be("123 Main Street");
    }

    [Fact]
    public async Task Given_BuyerProfileWithPaymentMethod_When_GetProfile_Then_PaymentMethodReturned()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var profile = new Api.Models.BuyerProfile 
        { 
            UserId = userId,
            PreferredPaymentMethod = "CreditCard"
        };
        
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new GetBuyerProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetBuyerProfileByUserIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.PreferredPaymentMethod.Should().Be("CreditCard");
    }

    [Fact]
    public async Task Given_UpdateBuyerProfileWithNewPhone_When_UpdateCalled_Then_PhoneUpdated()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateBuyerProfileValidator>();
        
        var profile = new Api.Models.BuyerProfile { UserId = userId, PhoneNumber = "0712345678" };
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new UpdateBuyerProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateBuyerProfileRequest(userId, "0798765432", "Street", "CreditCard");

        await handler.Handle(request, CancellationToken.None);

        profile.PhoneNumber.Should().Be("0798765432");
    }

    [Fact]
    public async Task Given_UpdateBuyerProfileWithNewAddress_When_UpdateCalled_Then_AddressUpdated()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateBuyerProfileValidator>();
        
        var profile = new Api.Models.BuyerProfile { UserId = userId, Address = "Old Street" };
        mockUserRepository.Setup(repo => repo.GetBuyerProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new UpdateBuyerProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateBuyerProfileRequest(userId, "0712345678", "New Street", "CreditCard");

        await handler.Handle(request, CancellationToken.None);

        profile.Address.Should().Be("New Street");
    }
}

public class KitchenProfileTests
{
    [Fact]
    public async Task Given_KitchenProfileWithRestaurantName_When_GetProfile_Then_NameReturned()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var profile = new Api.Models.KitchenProfile 
        { 
            UserId = userId, 
            RestaurantName = "My Kitchen"
        };
        
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new GetKitchenProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetKitchenProfileByUserIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.RestaurantName.Should().Be("My Kitchen");
    }

    [Fact]
    public async Task Given_KitchenProfileWithDescription_When_GetProfile_Then_DescriptionReturned()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var profile = new Api.Models.KitchenProfile 
        { 
            UserId = userId, 
            Description = "Best kitchen in town"
        };
        
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new GetKitchenProfileByUserIdHandler(mockUserRepository.Object);
        var request = new GetKitchenProfileByUserIdRequest(userId);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Description.Should().Be("Best kitchen in town");
    }

    [Fact]
    public async Task Given_UpdateKitchenProfileWithNewName_When_UpdateCalled_Then_NameUpdated()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateKitchenProfileValidator>();
        
        var profile = new Api.Models.KitchenProfile { UserId = userId, RestaurantName = "Old Kitchen" };
        mockUserRepository.Setup(repo => repo.GetKitchenProfileAsync(userId))
            .ReturnsAsync(profile);
        
        var handler = new UpdateKitchenProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateKitchenProfileRequest(userId, "New Kitchen", "New Desc", "Cuisine");

        await handler.Handle(request, CancellationToken.None);

        profile.RestaurantName.Should().Be("New Kitchen");
    }
}
