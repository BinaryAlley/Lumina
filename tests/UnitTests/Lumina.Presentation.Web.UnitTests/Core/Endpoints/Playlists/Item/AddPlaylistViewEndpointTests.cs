#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.Item;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.Item;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistViewEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";
    private const string CREATE_PLAYLIST_TITLE = "Create playlist";

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly AddPlaylistViewEndpoint _sut;
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();
    private readonly List<object?> _itemRouteValues = [];
    private readonly List<object?> _artistAlbumsRouteValues = [];
    private readonly List<object?> _albumTracksRouteValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistViewEndpointTests"/> class.
    /// </summary>
    public AddPlaylistViewEndpointTests()
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
            callInfo.Arg<string>() == "CreatePlaylist" ? CREATE_PLAYLIST_TITLE : $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[]
        {
            new("CreatePlaylist", CREATE_PLAYLIST_TITLE),
            new("Cancel", "Cancel")
        });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<AddPlaylistViewEndpoint>(_mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/playlists/item";

        StubUrls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldRenderThemedPageWithCreatePageMetadata()
    {
        // Arrange
        ThemePageRenderResultDto renderedPage = _themePageRenderResultDtoFixture.Create(
            content: "<section>create playlist</section>",
            script: "<script>create playlist</script>");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(renderedPage));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Shared/_ThemedView.cshtml", GetRenderedViewName(result));
        ThemeViewDto themedView = Assert.IsType<ThemeViewDto>(GetRenderedModel(result));
        Assert.Equal(renderedPage.Content, themedView.Content);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "playlists/item" &&
                page.Title == CREATE_PLAYLIST_TITLE &&
                page.Description == string.Empty &&
                Equals(page.PageData["isNew"], true) &&
                string.Equals(page.PageData["playlistId"] as string, Guid.Empty.ToString(), StringComparison.Ordinal) &&
                Equals(page.PageData["entriesPerPage"], 200) &&
                HasEmptyPlaylistJson(page)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldExposeEveryPageDataUrl()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                string.Equals(page.PageData["createPlaylistUrl"] as string, "http://localhost/en-us/playlists/api-create-playlist", StringComparison.Ordinal) &&
                string.Equals(page.PageData["itemUrl"] as string, $"http://localhost/en-us/playlists/item/{ZERO_GUID}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getEnabledLibrariesUrl"] as string, "http://localhost/en-us/libraries/manage/api-get-enabled-libraries", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getArtistsUrl"] as string, "http://localhost/en-us/library/audio-library/music-library/artists/api-get-artists", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getArtistAlbumsUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/api-get-albums", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getAlbumTracksUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/api-get-tracks", StringComparison.Ordinal) &&
                string.Equals(page.PageData["indexUrl"] as string, "http://localhost/en-us/playlists", StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
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
        // The page script replaces each all-zero Guid placeholder one at a time, so the route values must be empty Guids in route order.
        object? itemRouteValues = Assert.Single(_itemRouteValues);
        AssertRouteValueIsEmptyGuid(itemRouteValues, "playlistId");
        object? artistAlbumsRouteValues = Assert.Single(_artistAlbumsRouteValues);
        AssertRouteValueIsEmptyGuid(artistAlbumsRouteValues, "artistId");
        object? albumTracksRouteValues = Assert.Single(_albumTracksRouteValues);
        AssertRouteValueIsEmptyGuid(albumTracksRouteValues, "artistId");
        AssertRouteValueIsEmptyGuid(albumTracksRouteValues, "albumId");
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldExposeTheAntiforgeryTokenAndTheLocalizedStrings()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        _mockAntiforgery.Received(1).GetAndStoreTokens(_sut.HttpContext);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                string.Equals(page.PageData["antiforgeryToken"] as string, "request-token", StringComparison.Ordinal) &&
                HasDocumentedLocalizedStrings(page)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToTheRazorViewWithTheEmptyPlaylist()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Playlists/Item.cshtml", GetRenderedViewName(result));
        IReadOnlyDictionary<string, object?> viewData = GetRenderedViewData(result);
        Assert.Equal(CREATE_PLAYLIST_TITLE, viewData["Title"]);
        Assert.True((bool)viewData["IsNew"]!);
        PlaylistDto playlist = Assert.IsType<PlaylistDto>(GetRenderedModel(result));
        Assert.Equal(Guid.Empty, playlist.Id);
        Assert.Equal(string.Empty, playlist.Name);
        Assert.Null(playlist.Description);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.Empty(playlist.Entries.Data);
        Assert.Equal(1, playlist.Entries.CurrentPage);
        Assert.Equal(200, playlist.Entries.PerPage);
        Assert.Equal(0, playlist.Entries.Count);
        Assert.Equal(0, playlist.Entries.NumberOfPages);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldNotCallTheApi()
    {
        // Arrange
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Empty(_mockApiHttpClient.ReceivedCalls());
    }

    /// <summary>
    /// Stubs the absolute URLs that the endpoint resolves for the create playlist page, capturing the route values of the templated routes.
    /// </summary>
    private void StubUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.CREATE_PLAYLIST)
            .Returns("http://localhost/en-us/playlists/api-create-playlist");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.ITEM, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _itemRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/playlists/item/{ZERO_GUID}";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.GET_ENABLED_LIBRARIES)
            .Returns("http://localhost/en-us/libraries/manage/api-get-enabled-libraries");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTISTS)
            .Returns("http://localhost/en-us/library/audio-library/music-library/artists/api-get-artists");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTIST_ALBUMS, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _artistAlbumsRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/api-get-albums";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ALBUM_TRACKS, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _albumTracksRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/api-get-tracks";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.INDEX)
            .Returns("http://localhost/en-us/playlists");
    }

    /// <summary>
    /// Determines whether the serialized playlist of the page is the empty playlist the create page expects.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <returns><see langword="true"/> when the serialized playlist is empty and carries the default pagination shape; otherwise <see langword="false"/>.</returns>
    private static bool HasEmptyPlaylistJson(ThemePageDto page)
    {
        if (!page.PageData.TryGetValue("playlistJson", out object? value) || value is not string playlistJson)
            return false;
        using JsonDocument jsonDocument = JsonDocument.Parse(playlistJson);
        JsonElement root = jsonDocument.RootElement;
        JsonElement entries = root.GetProperty("entries");
        return root.GetProperty("id").GetGuid() == Guid.Empty
            && string.Equals(root.GetProperty("name").GetString(), string.Empty, StringComparison.Ordinal)
            && root.GetProperty("description").ValueKind == JsonValueKind.Null
            && string.Equals(root.GetProperty("type").GetString(), nameof(PlaylistType.Music), StringComparison.Ordinal)
            && entries.GetProperty("currentPage").GetInt32() == 1
            && entries.GetProperty("perPage").GetInt32() == 200
            && entries.GetProperty("count").GetInt32() == 0
            && entries.GetProperty("numberOfPages").GetInt32() == 0
            && entries.GetProperty("data").GetArrayLength() == 0;
    }

    /// <summary>
    /// Determines whether the page data carries the localized strings of the playlist page.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <returns><see langword="true"/> when the localized strings are exposed under the <c>strings</c> key; otherwise <see langword="false"/>.</returns>
    private static bool HasDocumentedLocalizedStrings(ThemePageDto page)
    {
        if (!page.PageData.TryGetValue("strings", out object? value) || value is not Dictionary<string, object?> strings)
            return false;
        return string.Equals(strings["CreatePlaylist"] as string, CREATE_PLAYLIST_TITLE, StringComparison.Ordinal)
            && string.Equals(strings["Cancel"] as string, "Cancel", StringComparison.Ordinal);
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
