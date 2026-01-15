using CampusEats.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;

namespace CampusEats.Test.Middleware;

public class CsrfTokenFilterMiddlewareTests
{
    [Fact]
    public async Task Given_SwaggerPath_When_InvokeAsync_Then_BypassesValidation()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        await middleware.InvokeAsync(context);

        mockNext.Verify(n => n(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Given_NoJwtCookie_When_InvokeAsync_Then_CallsNext()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        await middleware.InvokeAsync(context);

        mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task Given_JwtCookieWithoutCsrf_When_InvokeAsync_Then_ReturnsForbidden()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers.Cookie = "JWT=test-jwt-token";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Given_MatchingCsrfTokens_When_InvokeAsync_Then_CallsNext()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers.Cookie = "JWT=test-jwt; CSRF-TOKEN=matching-token";
        context.Request.Headers["X-CSRF-TOKEN"] = "matching-token";

        await middleware.InvokeAsync(context);

        mockNext.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("token1", "token2")]
    [InlineData("abc", "xyz")]
    public async Task Given_MismatchedCsrfTokens_When_InvokeAsync_Then_ReturnsForbidden(string cookieToken, string headerToken)
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers.Cookie = $"JWT=test-jwt; CSRF-TOKEN={cookieToken}";
        context.Request.Headers["X-CSRF-TOKEN"] = headerToken;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
        
        context.Response.Body.Position = 0;
        var reader = new StreamReader(context.Response.Body);
        var responseText = await reader.ReadToEndAsync();
        responseText.Should().Contain("CSRF validation failed");
    }

    [Fact]
    public async Task Given_JwtWithNullCsrfCookie_When_InvokeAsync_Then_ReturnsForbidden()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers.Cookie = "JWT=test-jwt";
        context.Request.Headers["X-CSRF-TOKEN"] = "some-token";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Given_JwtWithEmptyCsrfHeader_When_InvokeAsync_Then_ReturnsForbidden()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers.Cookie = "JWT=test-jwt; CSRF-TOKEN=token";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }

    [Theory]
    [InlineData("/swagger")]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/swagger/ui")]
    public async Task Given_SwaggerVariousPaths_When_InvokeAsync_Then_BypassesValidation(string path)
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await middleware.InvokeAsync(context);

        mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task Given_CaseSensitiveCsrfTokens_When_InvokeAsync_Then_ReturnsForbidden()
    {
        var mockNext = new Mock<RequestDelegate>();
        var middleware = new CsrfTokenFilterMiddleware(mockNext.Object);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers.Cookie = "JWT=test-jwt; CSRF-TOKEN=Token";
        context.Request.Headers["X-CSRF-TOKEN"] = "token";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }
}