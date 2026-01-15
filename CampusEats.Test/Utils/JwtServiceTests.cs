using System.IdentityModel.Tokens.Jwt;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.JwtUtil;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace CampusEats.Test.Utils;

public class JwtServiceTests
{
    private readonly IJwtService<User> _jwtService;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IBlackListTokenRepository> _mockBlackListTokenRepository;
    private readonly string _secretKey = "this-is-a-very-long-secret-key-for-testing-purposes-only-12345";

    public JwtServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockBlackListTokenRepository = new Mock<IBlackListTokenRepository>();
        
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns(_secretKey);
        
        _jwtService = new JwtService(_mockConfig.Object, new JwtSecurityTokenHandler(), _mockBlackListTokenRepository.Object);
    }

    private User CreateTestUser(string id = "00000000-0000-0000-0000-000000000001")
    {
        return new User
        {
            Id = Guid.Parse(id),
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = "hashed_password",
            Role = Role.Buyer
        };
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsJwtToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateToken_WithDifferentUsers_ReturnsDifferentTokens()
    {
        // Arrange
        var user1 = CreateTestUser("00000000-0000-0000-0000-000000000001");
        var user2 = CreateTestUser("00000000-0000-0000-0000-000000000002");

        // Act
        var token1 = _jwtService.GenerateToken(user1);
        var token2 = _jwtService.GenerateToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtFormat()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(token);

        // Assert
        canRead.Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_WithBuyerRole_IncludesRoleInToken()
    {
        // Arrange
        var user = CreateTestUser();
        user.Role = Role.Buyer;

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        jwtToken.Should().NotBeNull();
        var roleClain = jwtToken?.Claims.FirstOrDefault(c => c.Type == "role");
        roleClain?.Value.Should().Be(Role.Buyer.ToString());
    }

    [Fact]
    public void GenerateToken_WithKitchenRole_IncludesRoleInToken()
    {
        // Arrange
        var user = CreateTestUser();
        user.Role = Role.Kitchen;

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        jwtToken.Should().NotBeNull();
        var roleClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "role");
        roleClaim?.Value.Should().Be(Role.Kitchen.ToString());
    }

    [Fact]
    public void GenerateToken_IncludesUserIdInToken()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var user = CreateTestUser(userId.ToString());

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        jwtToken.Should().NotBeNull();
        var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "/id");
        userIdClaim?.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateToken_IncludesUserEmailInToken()
    {
        // Arrange
        var user = CreateTestUser();
        user.Email = "unique@example.com";

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        jwtToken.Should().NotBeNull();
        var emailClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "email");
        emailClaim?.Value.Should().Be("unique@example.com");
    }

    [Fact]
    public void GenerateToken_TokenHasExpirationTime()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        jwtToken.Should().NotBeNull();
        jwtToken?.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_WithNullUser_ThrowsArgumentNullException()
    {
        // Arrange
        User? nullUser = null;

        // Act
        var action = () => _jwtService.GenerateToken(nullUser!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
