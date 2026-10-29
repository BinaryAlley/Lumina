#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.Index;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.Index;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistsIndexViewEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly PlaylistsIndexViewEndpoint _sut;
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();
    private readonly List<object?> _itemRouteValues = [];
    private readonly List<object?> _deleteRouteValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistsIndexViewEndpointTests"/> class.
    /// </summary>
    public PlaylistsIndexViewEndpointTests()
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
            callInfo.Arg<string>() == "Title" ? "Playlists" : $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[]
        {
            new("Title", "Playlists"),
            new("NoPlaylists", "You have no playlists yet.")
        });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<PlaylistsIndexViewEndpoint>(_mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/playlists";

        StubUrls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldRenderThemedViewWithDocumentedPageData()
    {
        // Arrange
        ThemePageRenderResultDto renderedPage = _themePageRenderResultDtoFixture.Create(
            content: "<section>playlists</section>",
            script: "<script>playlists</script>");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(renderedPage));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Shared/_ThemedView.cshtml", GetRenderedViewName(result));
        ThemeViewDto themedView = Assert.IsType<ThemeViewDto>(GetRenderedModel(result));
        Assert.Equal(renderedPage.Content, themedView.Content);
        Assert.Equal(renderedPage.Script, themedView.Script);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "playlists/index" &&
                page.Title == "Playlists" &&
                page.Description == string.Empty &&
                string.Equals(page.PageData["getPlaylistsUrl"] as string, "http://localhost/en-us/playlists/api-get-playlists", StringComparison.Ordinal) &&
                string.Equals(page.PageData["addUrl"] as string, "http://localhost/en-us/playlists/item", StringComparison.Ordinal) &&
                string.Equals(page.PageData["itemUrl"] as string, $"http://localhost/en-us/playlists/item/{ZERO_GUID}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["deleteUrl"] as string, $"http://localhost/en-us/playlists/item/{ZERO_GUID}/api-delete-playlist", StringComparison.Ordinal) &&
                HasDocumentedLocalizedStrings(page)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldResolveTheAllZeroGuidTemplatesOfThePlaylistRoutes()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        // The playlist links replace the all-zero Guid placeholder, so the route values must carry an empty Guid.
        Assert.Single(_itemRouteValues);
        AssertRouteValueIsEmptyGuid(_itemRouteValues[0], "playlistId");
        Assert.Single(_deleteRouteValues);
        AssertRouteValueIsEmptyGuid(_deleteRouteValues[0], "playlistId");
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
        Assert.Equal("/Core/Views/Playlists/Index.cshtml", GetRenderedViewName(result));
        IReadOnlyDictionary<string, object?> viewData = GetRenderedViewData(result);
        Assert.Equal("Playlists", viewData["Title"]);
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
    /// Stubs the absolute URLs that the endpoint resolves for the playlists page.
    /// </summary>
    private void StubUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.GET_PLAYLISTS)
            .Returns("http://localhost/en-us/playlists/api-get-playlists");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.ADD)
            .Returns("http://localhost/en-us/playlists/item");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.ITEM, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _itemRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/playlists/item/{ZERO_GUID}";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.DELETE_PLAYLIST, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _deleteRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/playlists/item/{ZERO_GUID}/api-delete-playlist";
            });
    }

    /// <summary>
    /// Determines whether the page data carries the localized strings of the playlists page.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <returns><see langword="true"/> when the localized strings are exposed under the <c>strings</c> key; otherwise <see langword="false"/>.</returns>
    private static bool HasDocumentedLocalizedStrings(ThemePageDto page)
    {
        if (!page.PageData.TryGetValue("strings", out object? value) || value is not Dictionary<string, object?> strings)
            return false;
        return string.Equals(strings["Title"] as string, "Playlists", StringComparison.Ordinal)
            && string.Equals(strings["NoPlaylists"] as string, "You have no playlists yet.", StringComparison.Ordinal);
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
    /// Reads the private model of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The model passed to the rendered view.</returns>
    private static object? GetRenderedModel(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? modelField = typeof(RazorViewResult).GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(modelField);
        return modelField.GetValue(viewResult);
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
}
