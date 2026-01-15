using CampusEats.Api.Utils.CookieUtil;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Utils.CookieUtil;

public class CookieServiceTests
{
    [Theory]
    [InlineData("test-jwt-token")]
    [InlineData("another-token-123")]
    [InlineData("complex.jwt.token.with.dots")]
    public void Given_JwtToken_When_CreatingJwtCookie_Then_CookieIsAppended(string jwt)
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie(jwt, mockResponse.Object);

        mockCookies.Verify(c => c.Append("JWT", jwt, It.Is<CookieOptions>(opts =>
            opts.HttpOnly == true &&
            opts.Secure == false &&
            opts.SameSite == SameSiteMode.Lax &&
            opts.Path == "/"
        )), Times.Once);
    }

    [Theory]
    [InlineData("csrf-token-123")]
    [InlineData("test-csrf")]
    public void Given_CsrfToken_When_CreatingCsrfCookie_Then_CookieIsAppended(string token)
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateCsrfCookie(token, mockResponse.Object);

        mockCookies.Verify(c => c.Append("CSRF-TOKEN", token, It.Is<CookieOptions>(opts =>
            opts.HttpOnly == false &&
            opts.Secure == false &&
            opts.SameSite == SameSiteMode.Lax &&
            opts.Path == "/"
        )), Times.Once);
    }

    [Fact]
    public void Given_HttpResponse_When_DeletingJwtCookie_Then_EmptyCookieIsAppended()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.DeleteJwtCookie(mockResponse.Object);

        mockCookies.Verify(c => c.Append("JWT", "", It.Is<CookieOptions>(opts =>
            opts.HttpOnly == true &&
            opts.Secure == false
        )), Times.Once);
    }

    [Fact]
    public void Given_HttpResponse_When_DeletingCsrfCookie_Then_EmptyCookieIsAppended()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.DeleteCsrfCookie(mockResponse.Object);

        mockCookies.Verify(c => c.Append("CSRF-TOKEN", "", It.Is<CookieOptions>(opts =>
            opts.HttpOnly == false &&
            opts.Secure == false
        )), Times.Once);
    }

    [Fact]
    public void Given_EmptyJwtToken_When_CreatingJwtCookie_Then_CookieIsAppendedWithEmptyValue()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie("", mockResponse.Object);

        mockCookies.Verify(c => c.Append("JWT", "", It.IsAny<CookieOptions>()), Times.Once);
    }

    [Theory]
    [InlineData(SameSiteMode.Lax)]
    public void Given_DefaultConfiguration_When_CreatingCookie_Then_SameSiteIsCorrect(SameSiteMode expectedMode)
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie("test", mockResponse.Object);

        mockCookies.Verify(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), 
            It.Is<CookieOptions>(opts => opts.SameSite == expectedMode)), Times.Once);
    }

    [Fact]
    public void Given_JwtCookie_When_Created_Then_HttpOnlyIsTrue()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie("test-jwt", mockResponse.Object);

        mockCookies.Verify(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), 
            It.Is<CookieOptions>(opts => opts.HttpOnly == true)), Times.Once);
    }

    [Fact]
    public void Given_CsrfCookie_When_Created_Then_HttpOnlyIsFalse()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateCsrfCookie("test-csrf", mockResponse.Object);

        mockCookies.Verify(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), 
            It.Is<CookieOptions>(opts => opts.HttpOnly == false)), Times.Once);
    }

    [Fact]
    public void Given_Cookie_When_Deleted_Then_ExpirationIsInPast()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.DeleteJwtCookie(mockResponse.Object);

        mockCookies.Verify(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), 
            It.Is<CookieOptions>(opts => opts.Expires < DateTimeOffset.UtcNow)), Times.Once);
    }

    [Fact]
    public void Given_MultipleCookies_When_CreatingAndDeleting_Then_AllOperationsSucceed()
    {
        var mockResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

        CookieService.CreateJwtCookie("jwt-token", mockResponse.Object);
        CookieService.CreateCsrfCookie("csrf-token", mockResponse.Object);
        CookieService.DeleteJwtCookie(mockResponse.Object);
        CookieService.DeleteCsrfCookie(mockResponse.Object);

        mockCookies.Verify(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Exactly(4));
    }
}
