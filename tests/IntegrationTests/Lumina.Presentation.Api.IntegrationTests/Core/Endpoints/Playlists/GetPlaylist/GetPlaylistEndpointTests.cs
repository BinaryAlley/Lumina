#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylist;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private const string PATH = "/api/v1/users/me/playlists";

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPlaylistEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistExists_ShouldReturnItWithItsPageOfEntriesAndDensePositions()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid musicLibraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(musicLibraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(musicLibraryId, artistId, "Innuendo");
        Guid firstTrackId = await SeedTrackAsync(musicLibraryId, albumId, "Innuendo", 393);
        Guid secondTrackId = await SeedTrackAsync(musicLibraryId, albumId, "The Show Must Go On", 262);
        await SeedAlbumCoverAsync(albumId, "innuendo-cover.jpg");
        Guid deletedTrackId = Guid.NewGuid();
        Guid playlistId = await SeedPlaylistAsync(userId, "Evening listening");
        // The stored order keys are sparse, so the response must expose dense zero based display positions instead.
        await SeedPlaylistEntryAsync(playlistId, firstTrackId, 0);
        await SeedPlaylistEntryAsync(playlistId, secondTrackId, 1_000_000_000L);
        await SeedPlaylistEntryAsync(playlistId, deletedTrackId, 2_000_000_000L);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.Equal(playlistId, playlist!.Id);
        Assert.Equal("Evening listening", playlist.Name);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.Equal(3, playlist.Entries.Count);
        Assert.Equal(1, playlist.Entries.CurrentPage);
        Assert.Equal(3, playlist.Entries.PerPage);
        Assert.Equal(1, playlist.Entries.NumberOfPages);
        Assert.Equal([0, 1, 2], playlist.Entries.Data.Select(entry => entry.Position));

        PlaylistEntryResponse firstEntry = playlist.Entries.Data[0];
        Assert.Equal(firstTrackId, firstEntry.MediaItemId);
        Assert.Equal(PlaylistMediaItemType.MusicTrack, firstEntry.MediaItemType);
        Assert.Equal(musicLibraryId, firstEntry.LibraryId);
        Assert.Equal("Innuendo", firstEntry.MediaItemTitle);
        Assert.Equal(albumId, firstEntry.CollectionId);
        Assert.Equal("Innuendo", firstEntry.CollectionTitle);
        Assert.Equal(artistId, firstEntry.CreatorId);
        Assert.Equal("Queen", firstEntry.CreatorName);
        Assert.Equal(393, firstEntry.DurationInSeconds);
        Assert.Equal("innuendo-cover.jpg", firstEntry.CoverPath);

        PlaylistEntryResponse deletedEntry = playlist.Entries.Data[2];
        Assert.Equal(deletedTrackId, deletedEntry.MediaItemId);
        Assert.Equal(Guid.Empty, deletedEntry.LibraryId);
        Assert.Equal(string.Empty, deletedEntry.MediaItemTitle);
        Assert.Equal(Guid.Empty, deletedEntry.CollectionId);
        Assert.Equal(Guid.Empty, deletedEntry.CreatorId);
        Assert.Equal(0, deletedEntry.DurationInSeconds);
        Assert.Null(deletedEntry.CoverPath);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistIsEmpty_ShouldReturnAnEmptyPage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Nothing yet");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.Empty(playlist!.Entries.Data);
        Assert.Equal(0, playlist.Entries.Count);
        Assert.Equal(1, playlist.Entries.CurrentPage);
    }

    [Fact]
    public async Task GetPlaylist_WhenNoPaginationIsRequested_ShouldReturnEveryEntryInOnePage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Every entry");
        for (int index = 0; index < 4; index++)
            await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), index * 1_000_000_000L);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.Equal(1, playlist!.Entries.CurrentPage);
        Assert.Equal(4, playlist.Entries.PerPage);
        Assert.Equal(4, playlist.Entries.Count);
        Assert.Equal(1, playlist.Entries.NumberOfPages);
        Assert.Equal([0, 1, 2, 3], playlist.Entries.Data.Select(entry => entry.Position));
    }

    [Fact]
    public async Task GetPlaylist_WhenAPageIsRequested_ShouldReturnOnlyThatPageWithThePaginationMetadata()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Paged playlist");
        for (int index = 0; index < 3; index++)
            await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), index * 1_000_000_000L);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{playlistId}?currentPage=2&perPage=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.Equal(2, playlist!.Entries.CurrentPage);
        Assert.Equal(2, playlist.Entries.PerPage);
        Assert.Equal(3, playlist.Entries.Count);
        Assert.Equal(2, playlist.Entries.NumberOfPages);
        PlaylistEntryResponse entry = Assert.Single(playlist.Entries.Data);
        // The display position stays dense across pages, so the only entry of the second page is the third entry of the playlist.
        Assert.Equal(2, entry.Position);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistBelongsToAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{otherPlaylistId}");

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", $"{PATH}/{otherPlaylistId}");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{playlistId}");

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", $"{PATH}/{playlistId}");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/not-a-guid");

        // Assert
        await AssertValidationProblemAsync(response, $"{PATH}/not-a-guid", "PlaylistIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{Guid.Empty}");

        // Assert
        await AssertValidationProblemAsync(response, $"{PATH}/{Guid.Empty}", "PlaylistIdCannotBeEmpty");
    }

    /// <summary>
    /// Seeds a media library owned by <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the media library.</param>
    /// <returns>The Id of the seeded media library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId, userId: userId, title: $"Library-{Guid.NewGuid():N}", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        return libraryId;
    }

    /// <summary>
    /// Seeds an artist of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>The Id of the seeded artist.</returns>
    private async Task<Guid> SeedArtistAsync(Guid libraryId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid artistId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(
            id: artistId, libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        return artistId;
    }

    /// <summary>
    /// Seeds an album of the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="title">The title of the album.</param>
    /// <returns>The Id of the seeded album.</returns>
    private async Task<Guid> SeedAlbumAsync(Guid libraryId, Guid artistId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(
            id: albumId,
            artistId: artistId,
            libraryId: libraryId,
            title: title,
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        dbContext.Albums.Add(album);
        await dbContext.SaveChangesAsync();
        return albumId;
    }

    /// <summary>
    /// Seeds a track of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the track belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="title">The title of the track.</param>
    /// <param name="durationInSeconds">The duration of the track in seconds.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, Guid albumId, string title, int durationInSeconds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            title: title,
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        track.DurationInSeconds = durationInSeconds;
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        return trackId;
    }

    /// <summary>
    /// Seeds the cover artwork of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="albumId">The Id of the album that owns the artwork.</param>
    /// <param name="fileName">The file name of the stored cover.</param>
    private async Task SeedAlbumCoverAsync(Guid albumId, string fileName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album,
            ownerId: albumId,
            artworkType: ArtworkType.Cover,
            ordinal: 0,
            fileName: fileName));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid playlistId = Guid.NewGuid();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            type: PlaylistType.Music,
            entryCount: 0,
            version: 0,
            createdBy: userId));
        await dbContext.SaveChangesAsync();
        _seededPlaylistIds.Add(playlistId);
        return playlistId;
    }

    /// <summary>
    /// Seeds an entry of the playlist identified by <paramref name="playlistId"/>.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist the entry belongs to.</param>
    /// <param name="mediaItemId">The Id of the referenced media item.</param>
    /// <param name="position">The sparse order key of the entry.</param>
    private async Task SeedPlaylistEntryAsync(Guid playlistId, Guid mediaItemId, long position)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.PlaylistEntries.Add(_playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: mediaItemId,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: position));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and tracks it for cleanup.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUserIds.Add(userId);
        return userId;
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(candidate => candidate.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Deserializes the body of <paramref name="response"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the body to.</typeparam>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized body.</returns>
    private async Task<TValue?> DeserializeAsync<TValue>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TValue>(content, _jsonOptions);
    }

    /// <summary>
    /// Asserts that the response carries the cache headers of a private, authorization varying resource.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertCacheHeaders(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
    }

    /// <summary>
    /// Asserts that the response is a problem details document with the expected status, title and detail.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedStatus">The expected HTTP status.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedTitle, string expectedDetail, string expectedInstance)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)expectedStatus, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the response is a validation problem details document carrying the expected error code.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(422, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains(expectedErrorCode, errors!["General.Validation"]);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> libraryIds = [.. _seededLibraryIds];
            List<Guid> playlistIds = [.. _seededPlaylistIds];
            List<Guid> albumIds = await dbContext.Albums.Where(album => libraryIds.Contains(album.LibraryId)).Select(album => album.Id).ToListAsync();
            dbContext.PlaylistEntries.RemoveRange(dbContext.PlaylistEntries.Where(entry => playlistIds.Contains(entry.PlaylistId)));
            dbContext.Playlists.RemoveRange(dbContext.Playlists.Where(playlist => playlistIds.Contains(playlist.Id)));
            dbContext.MusicArtwork.RemoveRange(dbContext.MusicArtwork.Where(artwork => albumIds.Contains(artwork.OwnerId)));
            dbContext.Tracks.RemoveRange(dbContext.Tracks.Where(track => libraryIds.Contains(track.LibraryId)));
            dbContext.Albums.RemoveRange(dbContext.Albums.Where(album => libraryIds.Contains(album.LibraryId)));
            dbContext.Artists.RemoveRange(dbContext.Artists.Where(artist => libraryIds.Contains(artist.LibraryId)));
            dbContext.Libraries.RemoveRange(dbContext.Libraries.Where(library => libraryIds.Contains(library.Id)));
            dbContext.Users.RemoveRange(dbContext.Users.Where(user => _seededUserIds.Contains(user.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
