#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Tools;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Language.SetLanguage;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Localization;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Tools.Language.SetLanguage;

/// <summary>
/// Contains unit tests for the <see cref="SetLanguageEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLanguageEndpointTests
{
    private readonly IUrlService _mockUrlService;
    private readonly SetLanguageEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLanguageEndpointTests"/> class.
    /// </summary>
    public SetLanguageEndpointTests()
    {
        _mockUrlService = Substitute.For<IUrlService>();
        _sut = Factory.Create<SetLanguageEndpoint>(_mockUrlService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithoutReturnUrl_ShouldSetCultureCookieAndRedirectWithNewCulture()
    {
        // Arrange
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, "en-us");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED).Returns("http://localhost/en-us/");

        // Act
        IResult result = await _sut.ExecuteAsync(new SetLanguageRequest("de-DE", null), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("http://localhost/de-de/", redirectResult.Url);
        Assert.True(redirectResult.AcceptLocalUrlOnly);
        string setCookieHeader = _sut.HttpContext.Response.Headers.SetCookie.ToString();
        Assert.Contains(CookieRequestCultureProvider.DefaultCookieName, setCookieHeader);
        Assert.Contains("de-DE", setCookieHeader);
    }

    [Fact]
    public async Task ExecuteAsync_WhenReturnUrlProvided_ShouldReplaceCultureInReturnUrl()
    {
        // Arrange
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, "en-us");

        // Act
        IResult result = await _sut.ExecuteAsync(new SetLanguageRequest("de-DE", "/en-us/tools/settings"), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("/de-de/tools/settings", redirectResult.Url);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPathBasePresent_ShouldKeepPathBaseInRedirectUrl()
    {
        // Arrange
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, "en-us");
        _sut.HttpContext.Request.PathBase = "/lumina";

        // Act
        IResult result = await _sut.ExecuteAsync(new SetLanguageRequest("fr-FR", "/en-us/tools/settings"), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("/lumina/fr-fr/tools/settings", redirectResult.Url);
    }
}
