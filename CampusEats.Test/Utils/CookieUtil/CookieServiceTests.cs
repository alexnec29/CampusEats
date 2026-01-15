using CampusEats.Api.Utils.CookieUtil;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Utils.CookieUtil;

public class CookieServiceTests
{
    [Theory]
    [InlineData("test_jwt_token_123")]
    [InlineData("another_jwt_token_456")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9")]
    public void CreateJwtCookie_WithValidToken_ShouldAppendJwtCookie(string jwt)
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie(jwt, mockResponse.Object);

        mockCookies.Verify(c => c.Append(
            "JWT",
            jwt,
            It.Is<CookieOptions>(o => 
                o.HttpOnly == true && 
                o.Secure == false && 
                o.SameSite == SameSiteMode.Lax &&
                o.Path == "/"
            )
        ), Times.Once);
    }

    [Theory]
    [InlineData("csrf_token_abc123")]
    [InlineData("csrf_token_xyz789")]
    public void CreateCsrfCookie_WithValidToken_ShouldAppendCsrfCookie(string csrfToken)
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateCsrfCookie(csrfToken, mockResponse.Object);

        mockCookies.Verify(c => c.Append(
            "CSRF-TOKEN",
            csrfToken,
            It.Is<CookieOptions>(o => 
                o.HttpOnly == false && 
                o.Secure == false && 
                o.SameSite == SameSiteMode.Lax &&
                o.Path == "/"
            )
        ), Times.Once);
    }

    [Fact]
    public void DeleteJwtCookie_ShouldAppendExpiredJwtCookie()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.DeleteJwtCookie(mockResponse.Object);

        mockCookies.Verify(c => c.Append(
            "JWT",
            "",
            It.Is<CookieOptions>(o => 
                o.HttpOnly == true && 
                o.Secure == false && 
                o.SameSite == SameSiteMode.Lax &&
                o.Path == "/" &&
                o.Expires < DateTimeOffset.UtcNow
            )
        ), Times.Once);
    }

    [Fact]
    public void DeleteCsrfCookie_ShouldAppendExpiredCsrfCookie()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.DeleteCsrfCookie(mockResponse.Object);

        mockCookies.Verify(c => c.Append(
            "CSRF-TOKEN",
            "",
            It.Is<CookieOptions>(o => 
                o.HttpOnly == false && 
                o.Secure == false && 
                o.SameSite == SameSiteMode.Lax &&
                o.Path == "/" &&
                o.Expires < DateTimeOffset.UtcNow
            )
        ), Times.Once);
    }

    [Fact]
    public void CreateJwtCookie_WithEmptyToken_ShouldStillAppendCookie()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie("", mockResponse.Object);

        mockCookies.Verify(c => c.Append("JWT", "", It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public void CreateCsrfCookie_WithEmptyToken_ShouldStillAppendCookie()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateCsrfCookie("", mockResponse.Object);

        mockCookies.Verify(c => c.Append("CSRF-TOKEN", "", It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public void CookieOptions_ShouldHaveSecureFalseForDevelopment()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        CookieOptions? capturedOptions = null;

        mockCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
            .Callback<string, string, CookieOptions>((_, _, opts) => capturedOptions = opts);
        
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie("test", mockResponse.Object);

        capturedOptions.Should().NotBeNull();
        capturedOptions!.Secure.Should().BeFalse();
        capturedOptions.SameSite.Should().Be(SameSiteMode.Lax);
        capturedOptions.Path.Should().Be("/");
    }

    [Theory]
    [InlineData("JWT", true)]
    [InlineData("CSRF-TOKEN", false)]
    public void CreateCookies_ShouldSetCorrectHttpOnlyFlag(string cookieName, bool expectedHttpOnly)
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        CookieOptions? capturedOptions = null;

        mockCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
            .Callback<string, string, CookieOptions>((_, _, opts) => capturedOptions = opts);
        
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        if (cookieName == "JWT")
        {
            CookieService.CreateJwtCookie("test", mockResponse.Object);
        }
        else
        {
            CookieService.CreateCsrfCookie("test", mockResponse.Object);
        }

        capturedOptions.Should().NotBeNull();
        capturedOptions!.HttpOnly.Should().Be(expectedHttpOnly);
    }
}
