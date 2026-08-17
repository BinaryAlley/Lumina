#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
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
        Assert.Contains("not defined in the WebRoutes constants", exception.Message);
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
        DefaultHttpContext httpContext = CreateHttpContext(culture: "en-us", pathBase: "", host: "localhost:5012", scheme: "http");
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
        DefaultHttpContext httpContext = CreateHttpContext(culture: "en-us", pathBase: "", host: "localhost:5012", scheme: "http");
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
        DefaultHttpContext httpContext = CreateHttpContext(culture: "de-de", pathBase: "/lumina", host: "localhost:5012", scheme: "https");
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
        DefaultHttpContext httpContext = CreateHttpContext(culture: "en-us", pathBase: "", host: "localhost:5012", scheme: "http");
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        string? url = _sut.GetAbsoluteUrl(WebRoutes.FileSystem.GET_TYPE);

        // Assert
        Assert.Equal("http://localhost:5012/file-system/api-get-type", url);
    }

    private static DefaultHttpContext CreateHttpContext(string culture, string pathBase, string host, string scheme)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.RouteValues["culture"] = culture;
        httpContext.Request.PathBase = pathBase;
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.Scheme = scheme;
        return httpContext;
    }
}
