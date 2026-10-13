#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.Index;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.Index;

/// <summary>
/// Contains unit tests for the <see cref="MusicArtistsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistsIndexViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly MusicArtistsIndexViewEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly GetMusicArtistsViewRequestFixture _getMusicArtistsViewRequestFixture = new();
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtistsIndexViewEndpointTests"/> class.
    /// </summary>
    public MusicArtistsIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockUrlService = Substitute.For<IUrlService>();
        _mockThemePageRenderer = Substitute.For<ThemePageRenderer>(new ThemeService(_mockApiHttpClient), new ThemeTemplateEngine());
        _mockAntiforgery = Substitute.For<IAntiforgery>();
        _mockAntiforgery.GetAndStoreTokens(Arg.Any<HttpContext>())
            .Returns(new AntiforgeryTokenSet("request-token", "cookie-token", "__RequestVerificationToken", "RequestVerificationToken"));
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[] { new("Search", "Search") });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<MusicArtistsIndexViewEndpoint>(_mockApiHttpClient, _mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/library/audio-library/music-library/artists";
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdIsNull_ShouldRedirectToHome()
    {
        // Arrange
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED).Returns("http://localhost/en-us");

        // Act
        IResult result = await _sut.ExecuteAsync(_getMusicArtistsViewRequestFixture.Create(libraryId: null), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("http://localhost/en-us", redirectResult.Url);
        await _mockApiHttpClient.DidNotReceive().GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemePageRenderer.DidNotReceive().RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _mockAntiforgery.DidNotReceive().GetAndStoreTokens(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdProvided_ShouldRenderThemedPageThroughTheSharedView()
    {
        // Arrange
        LibraryDto expectedLibrary = _libraryDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedLibrary);
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTISTS).Returns("http://localhost/en-us/library/audio-library/music-library/artists/api-get-artists");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Settings.GET_USER_SETTINGS).Returns("http://localhost/en-us/tools/settings/api-get-user-settings");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.ARTIST, Arg.Any<object>()).Returns("http://localhost/en-us/library/audio-library/music-library/artists/00000000-0000-0000-0000-000000000000");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.DELETE_ARTIST, Arg.Any<object>()).Returns("http://localhost/en-us/library/audio-library/music-library/artists/00000000-0000-0000-0000-000000000000/api-delete-artist");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create(content: "<section>content</section>", script: "<script>script</script>")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getMusicArtistsViewRequestFixture.Create(libraryId: expectedLibrary.Id), CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockApiHttpClient.Received(1).GetAsync<LibraryDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", expectedLibrary.Id.ToString())),
            Arg.Any<CancellationToken>());
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "library/audio-library/music-library/artists/index" &&
                page.Title == expectedLibrary.Title &&
                page.PageData.ContainsKey("libraryId") &&
                page.PageData.ContainsKey("settingsUrl") &&
                page.PageData.ContainsKey("strings") &&
                string.Equals(
                    page.PageData["itemsUrl"] as string,
                    "http://localhost/en-us/library/audio-library/music-library/artists/api-get-artists",
                    StringComparison.Ordinal) &&
                string.Equals(
                    page.PageData["artistUrl"] as string,
                    "http://localhost/en-us/library/audio-library/music-library/artists/00000000-0000-0000-0000-000000000000",
                    StringComparison.Ordinal) &&
                string.Equals(
                    page.PageData["deleteUrl"] as string,
                    "http://localhost/en-us/library/audio-library/music-library/artists/00000000-0000-0000-0000-000000000000/api-delete-artist",
                    StringComparison.Ordinal)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
        _mockAntiforgery.Received(1).GetAndStoreTokens(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToRazorView()
    {
        // Arrange
        LibraryDto expectedLibrary = _libraryDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedLibrary);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getMusicArtistsViewRequestFixture.Create(libraryId: expectedLibrary.Id), CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
