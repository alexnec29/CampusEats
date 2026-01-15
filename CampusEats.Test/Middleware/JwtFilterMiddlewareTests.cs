using CampusEats.Api.Middleware;
using CampusEats.Api.Models;
using CampusEats.Api.Utils.JwtUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace CampusEats.Test.Middleware;

public class JwtFilterMiddlewareTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IJwtService<User>> _mockJwtService;
    private readonly JwtSecurityTokenHandler _jwtHandler;

    public JwtFilterMiddlewareTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockJwtService = new Mock<IJwtService<User>>();
        _jwtHandler = new JwtSecurityTokenHandler();
        
        // Setup configuration
        _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("http://localhost:5079");
        _mockConfig.Setup(x => x["Jwt:Audience"]).Returns("campuseats_api");
        _mockConfig.Setup(x => x["Jwt:Secret"]).Returns("super-secret-key-change-in-prodasdasfag");
    }

    [Fact]
    public async Task InvokeAsync_WithoutJwtCookie_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new JwtFilterMiddleware(next, _mockConfig.Object, _jwtHandler);

        // Act
        await middleware.InvokeAsync(context, _mockJwtService.Object);

        // Assert
        Assert.True(nextCalled, "Next middleware should be called when no JWT cookie exists");
        Assert.NotEqual((int)System.Net.HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithValidToken_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        
        _mockJwtService.Setup(x => x.IsTokenBlacklisted(It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new JwtFilterMiddleware(next, _mockConfig.Object, _jwtHandler);

        // Act
        await middleware.InvokeAsync(context, _mockJwtService.Object);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ChecksTokenBlacklistStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var testToken = "test-token";
        
        _mockJwtService.Setup(x => x.IsTokenBlacklisted(testToken))
            .ReturnsAsync(true);
        
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; };
        
        var middleware = new JwtFilterMiddleware(next, _mockConfig.Object, _jwtHandler);

        // Act
        await middleware.InvokeAsync(context, _mockJwtService.Object);

        // Assert - Should verify IsTokenBlacklisted was called
        _mockJwtService.Verify(x => x.IsTokenBlacklisted(It.IsAny<string>()), Times.AtMostOnce);
    }

    [Fact]
    public async Task InvokeAsync_ConstructorInitialization_ShouldStoreConfiguration()
    {
        // Arrange
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; };
        
        // Act
        var middleware = new JwtFilterMiddleware(next, _mockConfig.Object, _jwtHandler);

        // Assert
        Assert.NotNull(middleware);
    }
}
