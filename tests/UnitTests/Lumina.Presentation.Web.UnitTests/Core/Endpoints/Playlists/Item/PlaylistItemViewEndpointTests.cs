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
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
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
/// Contains unit tests for the <see cref="PlaylistItemViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistItemViewEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly PlaylistItemViewEndpoint _sut;
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedDtoFixture<PlaylistEntryDto> _paginatedDtoFixture = new();
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistItemViewEndpointTests"/> class.
    /// </summary>
    public PlaylistItemViewEndpointTests()
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
            $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[]
        {
            new("Save", "Save"),
            new("Delete", "Delete")
        });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<PlaylistItemViewEndpoint>(_mockApiHttpClient, _mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = $"/en-us/playlists/item/{s_playlistId}";

        StubUrls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIsReturned_ShouldRenderThemedPageWithPlaylistMetadata()
    {
        // Arrange
        PlaylistEntryDto expectedEntry = _playlistEntryDtoFixture.Create(
            position: 0,
            mediaItemTitle: "First track",
            mediaItemType: PlaylistMediaItemType.MusicTrack);
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(
            name: "Evening listening",
            description: "Quiet tracks for the end of the day.",
            entries: _paginatedDtoFixture.Create(
                data: [expectedEntry],
                currentPage: 1,
                perPage: 200,
                count: 1,
                numberOfPages: 1));
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        ThemePageRenderResultDto renderedPage = _themePageRenderResultDtoFixture.Create(
            content: "<section>playlist</section>",
            script: "<script>playlist</script>");
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
                page.Title == expectedPlaylist.Name &&
                page.Description == expectedPlaylist.Description &&
                Equals(page.PageData["isNew"], false) &&
                string.Equals(page.PageData["playlistId"] as string, expectedPlaylist.Id.ToString(), StringComparison.Ordinal) &&
                Equals(page.PageData["entriesPerPage"], 200) &&
                HasPlaylistJson(page, expectedPlaylist)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistHasNoDescription_ShouldRenderAnEmptyDescription()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(includeDescription: false);
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page => string.Equals(page.Description, string.Empty, StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIsReturned_ShouldExposeEveryPageDataUrl()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                string.Equals(page.PageData["createPlaylistUrl"] as string, "http://localhost/en-us/playlists/api-create-playlist", StringComparison.Ordinal) &&
                string.Equals(page.PageData["itemUrl"] as string, $"http://localhost/en-us/playlists/item/{ZERO_GUID}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getPlaylistUrl"] as string, "http://localhost/en-us/playlists/item/get-playlist", StringComparison.Ordinal) &&
                string.Equals(page.PageData["updatePlaylistUrl"] as string, "http://localhost/en-us/playlists/item/update-playlist", StringComparison.Ordinal) &&
                string.Equals(page.PageData["deletePlaylistUrl"] as string, "http://localhost/en-us/playlists/item/delete-playlist", StringComparison.Ordinal) &&
                string.Equals(page.PageData["addEntryUrl"] as string, "http://localhost/en-us/playlists/item/add-entry", StringComparison.Ordinal) &&
                string.Equals(page.PageData["removeEntryUrl"] as string, "http://localhost/en-us/playlists/item/remove-entry", StringComparison.Ordinal) &&
                string.Equals(page.PageData["moveEntryUrl"] as string, "http://localhost/en-us/playlists/item/move-entry", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getEnabledLibrariesUrl"] as string, "http://localhost/en-us/libraries/manage/api-get-enabled-libraries", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getArtistsUrl"] as string, "http://localhost/en-us/library/audio-library/music-library/artists/api-get-artists", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getArtistAlbumsUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/api-get-albums", StringComparison.Ordinal) &&
                string.Equals(page.PageData["getAlbumTracksUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/api-get-tracks", StringComparison.Ordinal) &&
                string.Equals(page.PageData["indexUrl"] as string, "http://localhost/en-us/playlists", StringComparison.Ordinal)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.ITEM, Arg.Is<object?>(routeValues => HasRouteValue(routeValues, "playlistId", Guid.Empty)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.GET_PLAYLIST, Arg.Is<object?>(routeValues => HasRouteValue(routeValues, "playlistId", expectedPlaylist.Id)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.UPDATE_PLAYLIST, Arg.Is<object?>(routeValues => HasRouteValue(routeValues, "playlistId", expectedPlaylist.Id)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.DELETE_PLAYLIST, Arg.Is<object?>(routeValues => HasRouteValue(routeValues, "playlistId", expectedPlaylist.Id)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.ADD_PLAYLIST_ENTRY, Arg.Is<object?>(routeValues => HasRouteValue(routeValues, "playlistId", expectedPlaylist.Id)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.REMOVE_PLAYLIST_ENTRY, Arg.Is<object?>(routeValues =>
            HasRouteValue(routeValues, "playlistId", expectedPlaylist.Id) && HasRouteValue(routeValues, "playlistEntryId", Guid.Empty)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Playlists.MOVE_PLAYLIST_ENTRY, Arg.Is<object?>(routeValues =>
            HasRouteValue(routeValues, "playlistId", expectedPlaylist.Id) && HasRouteValue(routeValues, "playlistEntryId", Guid.Empty)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Music.GET_ARTIST_ALBUMS, Arg.Is<object?>(routeValues => HasRouteValue(routeValues, "artistId", Guid.Empty)));
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Music.GET_ALBUM_TRACKS, Arg.Is<object?>(routeValues =>
            HasRouteValue(routeValues, "artistId", Guid.Empty) && HasRouteValue(routeValues, "albumId", Guid.Empty)));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIsReturned_ShouldExposeTheAntiforgeryTokenAndTheLocalizedStrings()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
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
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsAValidGuid_ShouldRequestThePlaylistOnce()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{expectedPlaylist.Id}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        SetRoutePlaylistId(s_playlistId.ToString().ToUpperInvariant());
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        SetRoutePlaylistId("not-a-guid");
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdRouteValueIsMissing_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("𝕡𝕝𝕒𝕪𝕝𝕚𝕤𝕥")] // unicode payload
    public async Task ExecuteAsync_WhenPlaylistIdIsCrafted_ShouldNormalizeItAndNeverForwardTheRawValue(string payload)
    {
        // Arrange
        SetRoutePlaylistId(payload);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToTheRazorViewWithThePlaylist()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(name: "Evening listening");
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Playlists/Item.cshtml", GetRenderedViewName(result));
        IReadOnlyDictionary<string, object?> viewData = GetRenderedViewData(result);
        Assert.Equal(expectedPlaylist.Name, viewData["Title"]);
        Assert.False((bool)viewData["IsNew"]!);
        PlaylistDto playlist = Assert.IsType<PlaylistDto>(GetRenderedModel(result));
        Assert.Equal(expectedPlaylist.Id, playlist.Id);
        Assert.Equal(expectedPlaylist.Name, playlist.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRoutePlaylistId(expectedPlaylist.Id);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                return expectedPlaylist;
            }, callInfo.ArgAt<CancellationToken>(1)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Stubs the absolute URLs that the endpoint resolves for the playlist page.
    /// </summary>
    private void StubUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.CREATE_PLAYLIST)
            .Returns("http://localhost/en-us/playlists/api-create-playlist");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.ITEM, Arg.Any<object?>())
            .Returns($"http://localhost/en-us/playlists/item/{ZERO_GUID}");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.GET_PLAYLIST, Arg.Any<object?>())
            .Returns("http://localhost/en-us/playlists/item/get-playlist");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.UPDATE_PLAYLIST, Arg.Any<object?>())
            .Returns("http://localhost/en-us/playlists/item/update-playlist");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.DELETE_PLAYLIST, Arg.Any<object?>())
            .Returns("http://localhost/en-us/playlists/item/delete-playlist");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.ADD_PLAYLIST_ENTRY, Arg.Any<object?>())
            .Returns("http://localhost/en-us/playlists/item/add-entry");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.REMOVE_PLAYLIST_ENTRY, Arg.Any<object?>())
            .Returns("http://localhost/en-us/playlists/item/remove-entry");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.MOVE_PLAYLIST_ENTRY, Arg.Any<object?>())
            .Returns("http://localhost/en-us/playlists/item/move-entry");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.GET_ENABLED_LIBRARIES)
            .Returns("http://localhost/en-us/libraries/manage/api-get-enabled-libraries");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTISTS)
            .Returns("http://localhost/en-us/library/audio-library/music-library/artists/api-get-artists");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTIST_ALBUMS, Arg.Any<object?>())
            .Returns($"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/api-get-albums");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.GET_ALBUM_TRACKS, Arg.Any<object?>())
            .Returns($"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/api-get-tracks");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Playlists.INDEX)
            .Returns("http://localhost/en-us/playlists");
    }

    /// <summary>
    /// Determines whether the serialized playlist of the page matches the playlist returned by the API.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <param name="expectedPlaylist">The playlist returned by the API.</param>
    /// <returns><see langword="true"/> when the serialized playlist matches the returned playlist; otherwise <see langword="false"/>.</returns>
    private static bool HasPlaylistJson(ThemePageDto page, PlaylistDto expectedPlaylist)
    {
        if (!page.PageData.TryGetValue("playlistJson", out object? value) || value is not string playlistJson)
            return false;
        using JsonDocument jsonDocument = JsonDocument.Parse(playlistJson);
        JsonElement root = jsonDocument.RootElement;
        JsonElement entries = root.GetProperty("entries");
        return root.GetProperty("id").GetGuid() == expectedPlaylist.Id
            && string.Equals(root.GetProperty("name").GetString(), expectedPlaylist.Name, StringComparison.Ordinal)
            && string.Equals(root.GetProperty("description").GetString(), expectedPlaylist.Description, StringComparison.Ordinal)
            && string.Equals(root.GetProperty("type").GetString(), nameof(PlaylistType.Music), StringComparison.Ordinal)
            && entries.GetProperty("currentPage").GetInt32() == expectedPlaylist.Entries.CurrentPage
            && entries.GetProperty("perPage").GetInt32() == expectedPlaylist.Entries.PerPage
            && entries.GetProperty("count").GetInt32() == expectedPlaylist.Entries.Count
            && entries.GetProperty("numberOfPages").GetInt32() == expectedPlaylist.Entries.NumberOfPages
            && entries.GetProperty("data").GetArrayLength() == expectedPlaylist.Entries.Data.Count;
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
        return string.Equals(strings["Save"] as string, "Save", StringComparison.Ordinal)
            && string.Equals(strings["Delete"] as string, "Delete", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the named property of an anonymous route values object holds the expected Guid.
    /// </summary>
    /// <param name="routeValues">The anonymous route values object passed to the URL service.</param>
    /// <param name="propertyName">The name of the route value property to check.</param>
    /// <param name="expected">The expected Guid value.</param>
    /// <returns><see langword="true"/> when the property holds the expected Guid; otherwise <see langword="false"/>.</returns>
    private static bool HasRouteValue(object? routeValues, string propertyName, Guid expected)
    {
        if (routeValues is null)
            return false;
        PropertyInfo? property = routeValues.GetType().GetProperty(propertyName);
        return property is not null && property.GetValue(routeValues) is Guid value && value == expected;
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

    /// <summary>
    /// Sets the route value of the playlist identifier route segment.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    private void SetRoutePlaylistId(object playlistId)
    {
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId;
    }
}
