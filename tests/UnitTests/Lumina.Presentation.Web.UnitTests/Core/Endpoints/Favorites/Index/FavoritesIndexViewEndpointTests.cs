#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.Index;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Favorites.Index;

/// <summary>
/// Contains unit tests for the <see cref="FavoritesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoritesIndexViewEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly FavoritesIndexViewEndpoint _sut;
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();
    private readonly List<object?> _artistRouteValues = [];
    private readonly List<object?> _albumRouteValues = [];
    private readonly List<object?> _trackRouteValues = [];
    private readonly List<object?> _bookRouteValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesIndexViewEndpointTests"/> class.
    /// </summary>
    public FavoritesIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockUrlService = Substitute.For<IUrlService>();
        _mockThemePageRenderer = Substitute.For<ThemePageRenderer>(new ThemeService(_mockApiHttpClient), new ThemeTemplateEngine());
        _mockAntiforgery = Substitute.For<IAntiforgery>();
        _mockAntiforgery.GetAndStoreTokens(Arg.Any<HttpContext>())
            .Returns(new AntiforgeryTokenSet("request-token", "cookie-token", "__RequestVerificationToken", "RequestVerificationToken"));
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(
            callInfo.Arg<string>(),
            callInfo.Arg<string>() == "Title" ? "Favorites" : $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[]
        {
            new("Title", "Favorites"),
            new("NoFavorites", "You have no favorites yet.")
        });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<FavoritesIndexViewEndpoint>(_mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/favorites";

        StubUrls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldRenderThemedViewWithDocumentedPageData()
    {
        // Arrange
        ThemePageRenderResultDto renderedPage = _themePageRenderResultDtoFixture.Create(
            content: "<section>favorites</section>",
            script: "<script>favorites</script>");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(renderedPage));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Shared/_ThemedView.cshtml", GetRenderedViewName(result));
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "favorites/index" &&
                page.Title == "Favorites" &&
                page.Description == string.Empty &&
                string.Equals(page.PageData["getFavoritesUrl"] as string, "http://localhost/en-us/favorites/api-get-favorites", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getFavoriteStatusUrl"] as string, "http://localhost/en-us/favorites/api-get-favorite-status", StringComparison.Ordinal) &&
                string.Equals(page.PageData["addFavoriteUrl"] as string, "http://localhost/en-us/favorites/api-add-favorite", StringComparison.Ordinal) &&
                string.Equals(page.PageData["removeFavoriteUrl"] as string, "http://localhost/en-us/favorites/{mediaKind}/{mediaItemId}/api-remove-favorite", StringComparison.Ordinal) &&
                string.Equals(page.PageData["artistUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["albumUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["trackUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/{ZERO_GUID}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["bookUrl"] as string, $"http://localhost/en-us/library/written-content-library/books-library/books/{ZERO_GUID}", StringComparison.Ordinal) &&
                HasDocumentedLocalizedStrings(page)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
        _mockAntiforgery.Received(1).GetAndStoreTokens(_sut.HttpContext);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldUseTheAllZeroGuidTemplatesOfTheMediaItemRoutes()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        // The detail pages replace each all-zero Guid placeholder one at a time, so the route values must be empty Guids in route order.
        Assert.Single(_artistRouteValues);
        AssertRouteValueIsEmptyGuid(_artistRouteValues[0], "artistId");
        Assert.Single(_albumRouteValues);
        AssertRouteValueIsEmptyGuid(_albumRouteValues[0], "artistId");
        AssertRouteValueIsEmptyGuid(_albumRouteValues[0], "albumId");
        Assert.Single(_trackRouteValues);
        AssertRouteValueIsEmptyGuid(_trackRouteValues[0], "artistId");
        AssertRouteValueIsEmptyGuid(_trackRouteValues[0], "albumId");
        AssertRouteValueIsEmptyGuid(_trackRouteValues[0], "trackId");
        Assert.Single(_bookRouteValues);
        AssertRouteValueIsEmptyGuid(_bookRouteValues[0], "bookId");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestAndExposeAFreshAntiforgeryToken()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        _mockAntiforgery.Received(1).GetAndStoreTokens(_sut.HttpContext);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page => string.Equals(page.PageData["antiforgeryToken"] as string, "request-token", StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToTheRazorViewWithTheLocalizedTitle()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Favorites/Index.cshtml", GetRenderedViewName(result));
        IReadOnlyDictionary<string, object?> viewData = GetRenderedViewData(result);
        Assert.Equal("Favorites", viewData["Title"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ThemePageRenderResultDto renderedPage = _themePageRenderResultDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return Result.From(renderedPage);
            }, callInfo.ArgAt<CancellationToken>(2)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Stubs the absolute URLs that the endpoint resolves for the favorites page.
    /// </summary>
    private void StubUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Favorites.GET_FAVORITES).Returns("http://localhost/en-us/favorites/api-get-favorites");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Favorites.GET_FAVORITE_STATUS).Returns("http://localhost/en-us/favorites/api-get-favorite-status");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Favorites.ADD_FAVORITE).Returns("http://localhost/en-us/favorites/api-add-favorite");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Favorites.REMOVE_FAVORITE).Returns("http://localhost/en-us/favorites/{mediaKind}/{mediaItemId}/api-remove-favorite");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.ARTIST, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _artistRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.ALBUM, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _albumRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.TRACK, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _trackRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/{ZERO_GUID}";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Books.EDIT_BOOK, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _bookRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/written-content-library/books-library/books/{ZERO_GUID}";
            });
    }

    /// <summary>
    /// Reads the private view name of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The name or path of the rendered view.</returns>
    private static string GetRenderedViewName(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? viewNameField = typeof(RazorViewResult).GetField("_viewName", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewNameField);
        return (string)viewNameField.GetValue(viewResult)!;
    }

    /// <summary>
    /// Reads the private view data of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The view data entries passed to the rendered view.</returns>
    private static IReadOnlyDictionary<string, object?> GetRenderedViewData(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? viewDataField = typeof(RazorViewResult).GetField("_viewData", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewDataField);
        return (IReadOnlyDictionary<string, object?>)viewDataField.GetValue(viewResult)!;
    }

    /// <summary>
    /// Determines whether the page data carries the localized strings of the favorites page.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <returns><see langword="true"/> when the localized strings are exposed under the <c>strings</c> key; otherwise <see langword="false"/>.</returns>
    private static bool HasDocumentedLocalizedStrings(ThemePageDto page)
    {
        if (!page.PageData.TryGetValue("strings", out object? value) || value is not Dictionary<string, object?> strings)
            return false;
        return string.Equals(strings["Title"] as string, "Favorites", StringComparison.Ordinal)
            && string.Equals(strings["NoFavorites"] as string, "You have no favorites yet.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the named property of an anonymous route values object is the empty Guid.
    /// </summary>
    /// <param name="routeValues">The anonymous route values object passed to the URL service.</param>
    /// <param name="propertyName">The name of the route value property to check.</param>
    private static void AssertRouteValueIsEmptyGuid(object? routeValues, string propertyName)
    {
        Assert.NotNull(routeValues);
        PropertyInfo? property = routeValues!.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.Equal(Guid.Empty, (Guid)property!.GetValue(routeValues)!);
    }
}
