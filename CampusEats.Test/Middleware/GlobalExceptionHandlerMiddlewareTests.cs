using CampusEats.Api.Middleware;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace CampusEats.Test.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task Given_NoException_When_InvokeAsync_Then_CallsNextDelegate()
    {
        var mockNext = new Mock<RequestDelegate>();
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task Given_ValidationException_When_InvokeAsync_Then_ReturnsBadRequest()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(new ValidationException("Validation failed"));

        var middleware = new GlobalExceptionHandlerMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Given_GenericException_When_InvokeAsync_Then_ReturnsInternalServerError()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Generic error"));

        var middleware = new GlobalExceptionHandlerMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Given_ExceptionInDevelopment_When_InvokeAsync_Then_ReturnsDetailedError()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var mockNext = new Mock<RequestDelegate>();
        var exception = new Exception("Development error");
        mockNext.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        var middleware = new GlobalExceptionHandlerMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
        context.Response.Body.Position = 0;
        var reader = new StreamReader(context.Response.Body);
        var responseText = await reader.ReadToEndAsync();
        responseText.Should().Contain("Development error");
    }

    [Fact]
    public async Task Given_Exception_When_InvokeAsync_Then_LogsError()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Test exception"));

        var middleware = new GlobalExceptionHandlerMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Given_ExceptionInProduction_When_InvokeAsync_Then_HidesDetailedError()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Sensitive information"));

        var middleware = new GlobalExceptionHandlerMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var reader = new StreamReader(context.Response.Body);
        var responseText = await reader.ReadToEndAsync();
        responseText.Should().Contain("An unexpected error occurred");
        responseText.Should().NotContain("Sensitive information");
    }
}