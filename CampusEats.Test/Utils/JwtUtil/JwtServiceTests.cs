using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.JwtUtil;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CampusEats.Test.Utils.JwtUtil;

public class JwtServiceTests
{
    [Fact]
    public async Task Given_User_WhenGenerateTokenIsCalled_ThenJwtIsReturned()
    {
        //Arrange
        Dictionary<string, string> inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Issuer", "Test-issuer"},
            {"Jwt:Audience", "Test-audience"},
            {"Jwt:Secret", "super-secret-testing-key-12345asdafjajfaf"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        Mock<IBlackListTokenRepository> blackListTokenRepositoryMock = new Mock<IBlackListTokenRepository>();

        JwtService jwtService = new JwtService(
            configuration,
            jwtSecurityTokenHandler,
            blackListTokenRepositoryMock.Object
            );

        User user = new User
        {
            Id = new Guid("c089c581-6eec-4895-ae55-a621f5fae11b"),
            Username = "validUsername",
            Email = "validEmail",
            Role = Role.Buyer
        };

        //Act
        string result = jwtService.GenerateToken(user);
        
        //Assert
        Assert.False(string.IsNullOrEmpty(result));

        var jsonToken = jwtSecurityTokenHandler.ReadJwtToken(result);
        Assert.Equal("Test-issuer", jsonToken.Issuer);
        Assert.Equal("Test-audience", jsonToken.Audiences.First());

        var claims = jsonToken.Claims.ToList();
        Assert.Contains(claims, c => c.Type == "/id" && c.Value == user.Id.ToString());
        Assert.Contains(claims, c => c.Type == ClaimTypes.Name && c.Value == user.Username);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        
        Assert.True(jsonToken.ValidTo > DateTime.UtcNow.AddMinutes(50));
        Assert.True(jsonToken.ValidTo < DateTime.UtcNow.AddMinutes(70));
    }
}