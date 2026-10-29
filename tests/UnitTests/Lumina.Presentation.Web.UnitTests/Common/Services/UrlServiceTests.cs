#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Services;

/// <summary>
/// Contains unit tests for the <see cref="UrlService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UrlServiceTests
{
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly UrlService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlServiceTests"/> class.
    /// </summary>
    public UrlServiceTests()
    {
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new UrlService(_mockHttpContextAccessor);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenRouteTemplateIsNotDefined_ShouldThrowArgumentException()
    {
        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.GetAbsoluteUrl("/not-a-real-route"));

        // Assert
        Assert.Equal("routeTemplate", exception.ParamName);
        Assert.Contains("The route template '/not-a-real-route' is not defined in the WebRoutes constants.", exception.Message);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        string? url = _sut.GetAbsoluteUrl(WebRoutes.Authentication.LOGIN);

        // Assert
        Assert.Null(url);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenCalled_ShouldReplaceCulturePlaceholderAndBuildAbsoluteUrl()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "en-us");
        httpContext.Request.Host = new HostString("localhost:5012");
        httpContext.Request.Scheme = "http";
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        string? url = _sut.GetAbsoluteUrl(WebRoutes.Authentication.LOGIN);

        // Assert
        Assert.Equal("http://localhost:5012/en-us/auth/api-login", url);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenCalledWithAdditionalRouteValues_ShouldReplacePlaceholders()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "en-us");
        httpContext.Request.Host = new HostString("localhost:5012");
        httpContext.Request.Scheme = "http";
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);
        Guid id = Guid.NewGuid();

        // Act
        string? url = _sut.GetAbsoluteUrl(WebRoutes.LibraryManagement.DELETE_LIBRARY, new { id });

        // Assert
        Assert.Equal($"http://localhost:5012/en-us/libraries/manage/api-item/{id}", url);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenPathBaseIsPresent_ShouldIncludePathBase()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "de-de");
        httpContext.Request.PathBase = "/lumina";
        httpContext.Request.Host = new HostString("localhost:5012");
        httpContext.Request.Scheme = "https";
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        string? url = _sut.GetAbsoluteUrl(WebRoutes.Authentication.REGISTER_VIEW);

        // Assert
        Assert.Equal("https://localhost:5012/lumina/de-de/auth/register", url);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenRouteTemplateDoesNotStartWithSlash_ShouldPrependSlash()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "en-us");
        httpContext.Request.Host = new HostString("localhost:5012");
        httpContext.Request.Scheme = "http";
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        string? url = _sut.GetAbsoluteUrl(WebRoutes.FileSystem.GET_TYPE);

        // Assert
        Assert.Equal("http://localhost:5012/file-system/api-get-type", url);
    }

    [Fact]
    public void GetRelativeUrl_WhenRouteTemplateIsNotDefined_ShouldThrowArgumentException()
    {
        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.GetRelativeUrl("/not-a-real-route"));

        // Assert
        Assert.Equal("routeTemplate", exception.ParamName);
        Assert.Contains("The route template '/not-a-real-route' is not defined in the WebRoutes constants.", exception.Message);
    }

    [Fact]
    public void GetRelativeUrl_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        string? url = _sut.GetRelativeUrl(WebRoutes.Authentication.LOGIN);

        // Assert
        Assert.Null(url);
    }

    [Fact]
    public void GetRelativeUrl_WhenCalled_ShouldReturnCultureLocalizedPathWithoutSchemeOrHost()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "en-us");
        httpContext.Request.Host = new HostString("localhost:5012");
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        string? url = _sut.GetRelativeUrl(WebRoutes.Authentication.LOGIN);

        // Assert
        Assert.Equal("/en-us/auth/api-login", url);
    }

    [Fact]
    public void GetRelativeUrl_WhenCultureRouteValueIsAbsent_ShouldFallBackToDefaultCulture()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(culture: null));

        // Act
        string? url = _sut.GetRelativeUrl(WebRoutes.Authentication.LOGIN);

        // Assert
        Assert.Equal("/en-US/auth/api-login", url);
    }

    [Fact]
    public void GetRelativeUrl_WhenCalledWithAdditionalRouteValues_ShouldReplacePlaceholders()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(culture: "en-us"));
        Guid id = Guid.NewGuid();

        // Act
        string? url = _sut.GetRelativeUrl(WebRoutes.LibraryManagement.DELETE_LIBRARY, new { id });

        // Assert
        Assert.Equal($"/en-us/libraries/manage/api-item/{id}", url);
    }

    [Fact]
    public void GetRelativeUrl_WhenRouteTemplateDoesNotStartWithSlash_ShouldPrependSlash()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(culture: "en-us"));

        // Act
        string? url = _sut.GetRelativeUrl(WebRoutes.FileSystem.GET_TYPE);

        // Assert
        Assert.Equal("/file-system/api-get-type", url);
    }

    [Fact]
    public void GetRelativeUrl_WhenPathBaseIsPresent_ShouldIncludePathBaseWithoutSchemeOrHost()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "de-de");
        httpContext.Request.PathBase = "/lumina";
        httpContext.Request.Host = new HostString("localhost:5012");
        httpContext.Request.Scheme = "https";
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        string? url = _sut.GetRelativeUrl(WebRoutes.Authentication.REGISTER_VIEW);

        // Assert
        Assert.Equal("/lumina/de-de/auth/register", url);
    }

    [Fact]
    public void GetAbsoluteUrl_WhenCalled_ShouldComposeSchemeAndHostWithRelativeUrl()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(culture: "de-de");
        httpContext.Request.PathBase = "/lumina";
        httpContext.Request.Host = new HostString("localhost:5012");
        httpContext.Request.Scheme = "https";
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);
        Guid id = Guid.NewGuid();

        // Act
        string? relativeUrl = _sut.GetRelativeUrl(WebRoutes.LibraryManagement.DELETE_LIBRARY, new { id });
        string? absoluteUrl = _sut.GetAbsoluteUrl(WebRoutes.LibraryManagement.DELETE_LIBRARY, new { id });

        // Assert
        Assert.NotNull(relativeUrl);
        Assert.Equal($"https://localhost:5012{relativeUrl}", absoluteUrl);
    }
}
