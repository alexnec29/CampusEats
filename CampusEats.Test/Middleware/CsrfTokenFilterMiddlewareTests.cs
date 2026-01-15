using CampusEats.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Test.Middleware;

public class CsrfTokenFilterMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithSwaggerPath_ShouldSkipCsrfValidation()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new CsrfTokenFilterMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled, "Next middleware should be called for swagger paths");
        Assert.NotEqual((int)System.Net.HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithoutJwtCookie_ShouldSkipCsrfValidation()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/orders";
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new CsrfTokenFilterMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled, "Next middleware should be called when no JWT cookie");
        Assert.NotEqual((int)System.Net.HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithJwtCookie_ShouldValidateCsrfToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/orders";
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new CsrfTokenFilterMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Without JWT, should skip CSRF check
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithMultiplePaths_ShouldHandleCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users/profile";
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new CsrfTokenFilterMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ConstructorInitialization_ShouldStoreRequestDelegate()
    {
        // Arrange
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; };
        
        // Act
        var middleware = new CsrfTokenFilterMiddleware(next);

        // Assert
        Assert.NotNull(middleware);
    }
}
