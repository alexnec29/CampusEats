using CampusEats.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CampusEats.Test.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WithoutException_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Development");
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) => { nextCalled = true; await Task.CompletedTask; };
        
        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, env.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled, "Next middleware should be called when no exception occurs");
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldHandleGracefully()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Production");
        
        var exception = new InvalidOperationException("Test error");
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; throw exception; };
        
        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, env.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)System.Net.HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetJsonContentType()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Production");
        
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; throw new Exception("Test"); };
        
        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, env.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteResponseBody()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Production");
        
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; throw new Exception("Test"); };
        
        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, env.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        var responseText = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.NotEmpty(responseText);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Production");
        
        RequestDelegate next = async (ctx) => { await Task.CompletedTask; throw new Exception("Test"); };
        
        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, env.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Logger should be called
        _mockLogger.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
