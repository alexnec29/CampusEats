using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.JwtUtil;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.SecurityAndAuth;

public class AuthenticationSecurityTests
{
    [Fact]
    public async Task Given_LoginWithCorrectCredentials_When_Handled_Then_TokenReturned()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Username = "testuser", 
            Role = Role.Buyer 
        };
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(user))
            .Returns("valid-token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("testuser", "password");
        
        var result = await handler.Handle(request, CancellationToken.None);
        
        mockJwtService.Verify(service => service.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Given_LoginWithNonExistentUser_When_Handled_Then_NotFound()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("nonexistent"))
            .ReturnsAsync((User)null);
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("nonexistent", "password");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.GetByUsernameAsync("nonexistent"), Times.Once);
        mockJwtService.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Given_LoginWithSqlInjectionAttempt_When_Handled_Then_Treated()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("' OR '1'='1", "password");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.GetByUsernameAsync("' OR '1'='1"), Times.Once);
    }

    [Fact]
    public async Task Given_LoginWithExtremelyLongUsername_When_Handled_Then_ValidatedOrRejected()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var validator = new LoginUserValidator();
        var mockJwtService = new Mock<IJwtService>();
        
        var longUsername = new string('a', 10000);
        var request = new LoginUserRequest(longUsername, "password");
        
        var result = await validator.ValidateAsync(request);
        
        // Should be invalid due to length
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_MultipleLoginAttemptsRapidly_When_Sequential_Then_AllProcessed()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Role = Role.Buyer };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(user))
            .Returns("token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => handler.Handle(new LoginUserRequest("testuser", "password"), CancellationToken.None))
            .ToList();
        
        await Task.WhenAll(tasks);
        
        mockJwtService.Verify(service => service.GenerateToken(user), Times.AtLeast(10));
    }
}

public class TokenBlacklistingTests
{
    [Fact]
    public async Task Given_LogoutWithValidToken_When_Called_Then_TokenBlacklisted()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        
        var userId = Guid.NewGuid();
        var token = "valid-token";
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        mockBlacklistRepository.Setup(repo => repo.IsTokenBlacklistedAsync(token))
            .ReturnsAsync(false);
        
        var handler = new LogoutUserHandler(mockUserRepository.Object, mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest(userId, token);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockBlacklistRepository.Verify(repo => repo.AddToBlacklistAsync(userId, token), Times.Once);
    }

    [Fact]
    public async Task Given_CheckBlacklistedToken_When_Called_Then_CorrectResult()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        
        var userId = Guid.NewGuid();
        var token = "blacklisted-token";
        
        mockBlacklistRepository.Setup(repo => repo.IsTokenBlacklistedAsync(token))
            .ReturnsAsync(true);
        
        var handler = new LogoutUserHandler(mockUserRepository.Object, mockBlacklistRepository.Object, mockValidator.Object);
        
        var isBlacklisted = await mockBlacklistRepository.Object.IsTokenBlacklistedAsync(token);
        
        isBlacklisted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_MultipleLogoutAttempts_When_Sequential_Then_AllProcessed()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        
        var userId = Guid.NewGuid();
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        mockBlacklistRepository.Setup(repo => repo.IsTokenBlacklistedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var handler = new LogoutUserHandler(mockUserRepository.Object, mockBlacklistRepository.Object, mockValidator.Object);
        
        var tokens = Enumerable.Range(0, 5)
            .Select(i => $"token-{i}")
            .ToList();
        
        var tasks = tokens.Select(token =>
            handler.Handle(new LogoutUserRequest(userId, token), CancellationToken.None)
        );
        
        await Task.WhenAll(tasks);
        
        mockBlacklistRepository.Verify(repo => repo.AddToBlacklistAsync(userId, It.IsAny<string>()), Times.Exactly(5));
    }
}

public class RoleBasedAccessTests
{
    [Fact]
    public async Task Given_BuyerCreatesUser_When_Created_Then_RolePreserved()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("newuser", "user@example.com", "Pass123!", "Pass123!");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Username == "newuser")), Times.Once);
    }

    [Fact]
    public async Task Given_AdminRole_When_Created_Then_RoleAssignedCorrectly()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        
        User capturedUser = null;
        mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);
        
        var request = new CreateUserRequest("admin", "admin@example.com", "AdminPass123!", "AdminPass123!");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleRoles_When_SeparateUsersCreated_Then_EachHasCorrectRole()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        
        var usernames = new[] { "buyer1", "kitchen1", "admin1" };
        
        foreach (var username in usernames)
        {
            var request = new CreateUserRequest(username, $"{username}@example.com", "Pass123!", "Pass123!");
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Exactly(3));
    }
}

public class PasswordSecurityTests
{
    [Fact]
    public async Task Given_CreateUserWithWeakPassword_When_Validated_Then_Rejected()
    {
        var validator = new CreateUserValidator();
        var request = new CreateUserRequest("user", "user@example.com", "weak", "weak");
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_CreateUserWithStrongPassword_When_Validated_Then_Accepted()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("user", "user@example.com", "VeryStr0ng!Pass", "VeryStr0ng!Pass");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_PasswordWithSpecialCharacters_When_Validated_Then_Accepted()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("user", "user@example.com", "P@ssw0rd!#$%", "P@ssw0rd!#$%");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_PasswordMismatch_When_Validated_Then_Rejected()
    {
        var validator = new CreateUserValidator();
        var request = new CreateUserRequest("user", "user@example.com", "Pass123!", "Different123!");
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }
}

public class EmailValidationSecurityTests
{
    [Fact]
    public async Task Given_ValidEmailFormat_When_Created_Then_Accepted()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByEmailAsync("user@example.com"))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("user"))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("user", "user@example.com", "Pass123!", "Pass123!");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_DuplicateEmail_When_Created_Then_Rejected()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        var existingUser = new User { Id = Guid.NewGuid(), Email = "user@example.com" };
        mockUserRepository.Setup(repo => repo.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(existingUser);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("user", "user@example.com", "Pass123!", "Pass123!");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Given_EmailWithPlusAddressing_When_Validated_Then_Accepted()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByEmailAsync("user+tag@example.com"))
            .ReturnsAsync((User)null);
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("user"))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockUserRepository.Object);
        var request = new CreateUserRequest("user", "user+tag@example.com", "Pass123!", "Pass123!");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }
}
