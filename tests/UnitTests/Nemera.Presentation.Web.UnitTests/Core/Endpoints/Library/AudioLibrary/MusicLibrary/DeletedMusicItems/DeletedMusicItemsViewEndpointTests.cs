#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.DeletedMusicItems;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.DeletedMusicItems;

/// <summary>
/// Contains unit tests for the <see cref="DeletedMusicItemsViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicItemsViewEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly DeletedMusicItemsViewEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly DeletedMusicArtistDtoFixture _deletedMusicArtistDtoFixture = new();
    private readonly DeletedMusicAlbumDtoFixture _deletedMusicAlbumDtoFixture = new();
    private readonly DeletedMusicTrackDtoFixture _deletedMusicTrackDtoFixture = new();
    private readonly DeletedMusicItemsPageDtoFixture _deletedMusicItemsPageDtoFixture = new();
    private readonly GetMusicArtistsViewRequestFixture _getMusicArtistsViewRequestFixture = new();
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();
    private readonly List<object?> _artistRestoreRouteValues = [];
    private readonly List<object?> _albumRestoreRouteValues = [];
    private readonly List<object?> _trackRestoreRouteValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedMusicItemsViewEndpointTests"/> class.
    /// </summary>
    public DeletedMusicItemsViewEndpointTests()
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
            callInfo.Arg<string>() == "Title" ? "Deleted items" : $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(
        [
            new("Title", "Deleted items"),
            new("BackToLibrary", "Back to library"),
            new("NoDeletedItems", "There are no deleted items in this library."),
            new("Artists", "Artists"),
            new("Albums", "Albums"),
            new("Tracks", "Tracks"),
            new("Name", "Name"),
            new("Artist", "Artist"),
            new("Album", "Album"),
            new("TrackTitle", "Title"),
            new("DeletedOn", "Deleted on"),
            new("Restore", "Restore"),
            new("Restored", "The item was restored."),
            new("RestoreFailed", "The item could not be restored.")
        ]);
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<DeletedMusicItemsViewEndpoint>(_mockApiHttpClient, _mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/library/audio-library/music-library/deleted-items";

        StubUrls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldRenderThemedViewWithDocumentedPageData()
    {
        // Arrange
        LibraryDto library = _libraryDtoFixture.Create(title: "Music");
        DeletedMusicArtistDto[] deletedArtists = [.. _deletedMusicArtistDtoFixture.CreateMany(2)];
        DeletedMusicAlbumDto[] deletedAlbums = [.. _deletedMusicAlbumDtoFixture.CreateMany(2)];
        DeletedMusicTrackDto[] deletedTracks = [.. _deletedMusicTrackDtoFixture.CreateMany(2)];
        Guid libraryId = library.Id!.Value;
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(library);
        _mockApiHttpClient.GetAsync<DeletedMusicArtistDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedArtists);
        _mockApiHttpClient.GetAsync<DeletedMusicAlbumDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedAlbums);
        _mockApiHttpClient.GetAsync<DeletedMusicTrackDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedTracks);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create(content: "<section>deleted</section>", script: "<script>deleted</script>")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getMusicArtistsViewRequestFixture.Create(libraryId: libraryId), CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Shared/_ThemedView.cshtml", GetRenderedViewName(result));
        await _mockApiHttpClient.Received(1).GetAsync<LibraryDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", libraryId.ToString())),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<DeletedMusicArtistDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Music.GET_DELETED_ARTISTS.Replace("{libraryId}", libraryId.ToString())),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<DeletedMusicAlbumDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Music.GET_DELETED_ALBUMS.Replace("{libraryId}", libraryId.ToString())),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<DeletedMusicTrackDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Music.GET_DELETED_TRACKS.Replace("{libraryId}", libraryId.ToString())),
            Arg.Any<CancellationToken>());
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "library/audio-library/music-library/deleted-items" &&
                page.Title == "Deleted items" &&
                page.Description == string.Empty &&
                string.Equals(page.PageData["libraryId"] as string, libraryId.ToString(), StringComparison.Ordinal) &&
                string.Equals(page.PageData["libraryTitle"] as string, "Music", StringComparison.Ordinal) &&
                string.Equals(page.PageData["artistRestoreUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/api-restore-artist", StringComparison.Ordinal) &&
                string.Equals(page.PageData["albumRestoreUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/api-restore-album", StringComparison.Ordinal) &&
                string.Equals(page.PageData["trackRestoreUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/{ZERO_GUID}/api-restore-track", StringComparison.Ordinal) &&
                string.Equals(page.PageData["backUrl"] as string, $"http://localhost/en-us/library/audio-library/music-library/artists?libraryId={libraryId}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["antiforgeryToken"] as string, "request-token", StringComparison.Ordinal) &&
                HasDocumentedDeletedMusicItems(page, deletedArtists, deletedAlbums, deletedTracks) &&
                HasDocumentedLocalizedStrings(page)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
        _mockAntiforgery.Received(1).GetAndStoreTokens(_sut.HttpContext);
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
        await _mockApiHttpClient.DidNotReceive().GetAsync<DeletedMusicArtistDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().GetAsync<DeletedMusicAlbumDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().GetAsync<DeletedMusicTrackDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemePageRenderer.DidNotReceive().RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _mockAntiforgery.DidNotReceive().GetAndStoreTokens(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToTheRazorViewWithTheDeletedMusicItems()
    {
        // Arrange
        LibraryDto library = _libraryDtoFixture.Create(title: "Music");
        DeletedMusicArtistDto[] deletedArtists = [.. _deletedMusicArtistDtoFixture.CreateMany(2)];
        DeletedMusicAlbumDto[] deletedAlbums = [.. _deletedMusicAlbumDtoFixture.CreateMany(2)];
        DeletedMusicTrackDto[] deletedTracks = [.. _deletedMusicTrackDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(library);
        _mockApiHttpClient.GetAsync<DeletedMusicArtistDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedArtists);
        _mockApiHttpClient.GetAsync<DeletedMusicAlbumDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedAlbums);
        _mockApiHttpClient.GetAsync<DeletedMusicTrackDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedTracks);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getMusicArtistsViewRequestFixture.Create(libraryId: library.Id), CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Library/AudioLibrary/MusicLibrary/DeletedItems.cshtml", GetRenderedViewName(result));
        DeletedMusicItemsPageDto expectedPage = _deletedMusicItemsPageDtoFixture.Create(
            library: library,
            deletedArtists: deletedArtists,
            deletedAlbums: deletedAlbums,
            deletedTracks: deletedTracks);
        DeletedMusicItemsPageDto model = Assert.IsType<DeletedMusicItemsPageDto>(GetRenderedModel(result));
        Assert.Equal(expectedPage, model);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldUseTheAllZeroGuidTemplatesOfTheRestoreRoutes()
    {
        // Arrange
        LibraryDto library = _libraryDtoFixture.Create(title: "Music");
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(library);
        _mockApiHttpClient.GetAsync<DeletedMusicArtistDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<DeletedMusicArtistDto>());
        _mockApiHttpClient.GetAsync<DeletedMusicAlbumDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<DeletedMusicAlbumDto>());
        _mockApiHttpClient.GetAsync<DeletedMusicTrackDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<DeletedMusicTrackDto>());
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create()));

        // Act
        await _sut.ExecuteAsync(_getMusicArtistsViewRequestFixture.Create(libraryId: library.Id), CancellationToken.None);

        // Assert
        // The themed page replaces each all-zero Guid placeholder of the restore URLs one at a time, so every route value must be an empty Guid.
        Assert.Single(_artistRestoreRouteValues);
        AssertRouteValueIsEmptyGuid(_artistRestoreRouteValues[0], "artistId");
        Assert.Single(_albumRestoreRouteValues);
        AssertRouteValueIsEmptyGuid(_albumRestoreRouteValues[0], "artistId");
        AssertRouteValueIsEmptyGuid(_albumRestoreRouteValues[0], "albumId");
        Assert.Single(_trackRestoreRouteValues);
        AssertRouteValueIsEmptyGuid(_trackRestoreRouteValues[0], "artistId");
        AssertRouteValueIsEmptyGuid(_trackRestoreRouteValues[0], "albumId");
        AssertRouteValueIsEmptyGuid(_trackRestoreRouteValues[0], "trackId");
    }

    /// <summary>
    /// Stubs the absolute URLs that the endpoint resolves for the deleted music items page.
    /// </summary>
    private void StubUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.ARTISTS_INDEX).Returns("http://localhost/en-us/library/audio-library/music-library/artists");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.RESTORE_ARTIST, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _artistRestoreRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/api-restore-artist";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.RESTORE_ALBUM, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _albumRestoreRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/api-restore-album";
            });
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Music.RESTORE_TRACK, Arg.Any<object?>())
            .Returns(callInfo =>
            {
                _trackRestoreRouteValues.Add(callInfo.ArgAt<object?>(1));
                return $"http://localhost/en-us/library/audio-library/music-library/artists/{ZERO_GUID}/albums/{ZERO_GUID}/tracks/{ZERO_GUID}/api-restore-track";
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
    /// Determines whether the page data carries the serialized deleted music items of the page.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <param name="deletedArtists">The deleted artists expected in the page data.</param>
    /// <param name="deletedAlbums">The deleted albums expected in the page data.</param>
    /// <param name="deletedTracks">The deleted tracks expected in the page data.</param>
    /// <returns><see langword="true"/> when the serialized deleted items match the expected ones; otherwise <see langword="false"/>.</returns>
    private static bool HasDocumentedDeletedMusicItems(
        ThemePageDto page,
        DeletedMusicArtistDto[] deletedArtists,
        DeletedMusicAlbumDto[] deletedAlbums,
        DeletedMusicTrackDto[] deletedTracks)
    {
        if (!page.PageData.TryGetValue("deletedArtistsJson", out object? artistsValue) || artistsValue is not string artistsJson)
            return false;
        if (!page.PageData.TryGetValue("deletedAlbumsJson", out object? albumsValue) || albumsValue is not string albumsJson)
            return false;
        if (!page.PageData.TryGetValue("deletedTracksJson", out object? tracksValue) || tracksValue is not string tracksJson)
            return false;

        using JsonDocument artistsDocument = JsonDocument.Parse(artistsJson);
        using JsonDocument albumsDocument = JsonDocument.Parse(albumsJson);
        using JsonDocument tracksDocument = JsonDocument.Parse(tracksJson);
        return artistsDocument.RootElement.GetArrayLength() == deletedArtists.Length
            && string.Equals(artistsDocument.RootElement[0].GetProperty("name").GetString(), deletedArtists[0].Name, StringComparison.Ordinal)
            && albumsDocument.RootElement.GetArrayLength() == deletedAlbums.Length
            && string.Equals(albumsDocument.RootElement[0].GetProperty("title").GetString(), deletedAlbums[0].Title, StringComparison.Ordinal)
            && tracksDocument.RootElement.GetArrayLength() == deletedTracks.Length
            && string.Equals(tracksDocument.RootElement[0].GetProperty("albumTitle").GetString(), deletedTracks[0].AlbumTitle, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the page data carries the localized strings of the deleted music items page, both raw and serialized.
    /// </summary>
    /// <param name="page">The page model passed to the theme renderer.</param>
    /// <returns><see langword="true"/> when the localized strings are exposed and serialized; otherwise <see langword="false"/>.</returns>
    private static bool HasDocumentedLocalizedStrings(ThemePageDto page)
    {
        if (!page.PageData.TryGetValue("strings", out object? value) || value is not Dictionary<string, object?> strings)
            return false;
        if (!page.PageData.TryGetValue("stringsJson", out object? jsonValue) || jsonValue is not string stringsJson)
            return false;

        using JsonDocument stringsDocument = JsonDocument.Parse(stringsJson);
        return string.Equals(strings["Title"] as string, "Deleted items", StringComparison.Ordinal)
            && string.Equals(strings["NoDeletedItems"] as string, "There are no deleted items in this library.", StringComparison.Ordinal)
            && string.Equals(stringsDocument.RootElement.GetProperty("title").GetString(), "Deleted items", StringComparison.Ordinal)
            && string.Equals(stringsDocument.RootElement.GetProperty("noDeletedItems").GetString(), "There are no deleted items in this library.", StringComparison.Ordinal);
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
