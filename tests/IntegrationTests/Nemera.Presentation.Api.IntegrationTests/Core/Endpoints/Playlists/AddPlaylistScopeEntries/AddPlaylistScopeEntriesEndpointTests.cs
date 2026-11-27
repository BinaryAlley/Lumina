#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistScopeEntries;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.AddPlaylistScopeEntries;

/// <summary>
/// Contains integration tests for the <see cref="AddPlaylistScopeEntriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly AddPlaylistScopeEntriesRequestFixture _addPlaylistScopeEntriesRequestFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddPlaylistScopeEntriesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task AddPlaylistScopeEntries_WhenTheWholeLibraryIsScoped_ShouldAppendEveryTrackInCatalogueOrder()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid alphaArtistId = await SeedArtistAsync(libraryId, "Alpha Artist");
        Guid alphaAlbumId = await SeedAlbumAsync(libraryId, alphaArtistId, "Alpha Album");
        Guid alphaFirstTrackId = await SeedTrackAsync(libraryId, alphaAlbumId, "Alpha One", 100, trackNumber: 1);
        Guid alphaSecondTrackId = await SeedTrackAsync(libraryId, alphaAlbumId, "Alpha Two", 110, trackNumber: 2);
        Guid betaArtistId = await SeedArtistAsync(libraryId, "Beta Artist");
        Guid betaAlbumId = await SeedAlbumAsync(libraryId, betaArtistId, "Beta Album");
        Guid betaTrackId = await SeedTrackAsync(libraryId, betaAlbumId, "Beta One", 120, trackNumber: 1);
        Guid playlistId = await SeedPlaylistAsync(userId, "Whole library", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: libraryId);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries-scope", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        PlaylistEntriesAddedResponse? addedEntries = await DeserializeAsync<PlaylistEntriesAddedResponse>(response);
        Assert.NotNull(addedEntries);
        Assert.Equal(playlistId, addedEntries!.PlaylistId);
        Assert.Equal(3, addedEntries.AddedCount);
        Assert.NotEqual(default, addedEntries.UpdatedOnUtc);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<Guid> storedTrackIds = await GetOrderedTrackIdsAsync(dbContext, playlistId);
        Assert.Equal([alphaFirstTrackId, alphaSecondTrackId, betaTrackId], storedTrackIds);
        PlaylistEntity playlist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(3, playlist.EntryCount);
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenAnArtistIsScoped_ShouldAppendOnlyThatArtistsTracks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid alphaArtistId = await SeedArtistAsync(libraryId, "Alpha Artist");
        Guid alphaAlbumId = await SeedAlbumAsync(libraryId, alphaArtistId, "Alpha Album");
        Guid alphaFirstTrackId = await SeedTrackAsync(libraryId, alphaAlbumId, "Alpha One", 100, trackNumber: 1);
        Guid alphaSecondTrackId = await SeedTrackAsync(libraryId, alphaAlbumId, "Alpha Two", 110, trackNumber: 2);
        Guid betaArtistId = await SeedArtistAsync(libraryId, "Beta Artist");
        Guid betaAlbumId = await SeedAlbumAsync(libraryId, betaArtistId, "Beta Album");
        await SeedTrackAsync(libraryId, betaAlbumId, "Beta One", 120, trackNumber: 1);
        Guid playlistId = await SeedPlaylistAsync(userId, "One artist", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(
            libraryId: libraryId, artistId: alphaArtistId, includeArtistId: true);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries-scope", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistEntriesAddedResponse? addedEntries = await DeserializeAsync<PlaylistEntriesAddedResponse>(response);
        Assert.NotNull(addedEntries);
        Assert.Equal(2, addedEntries!.AddedCount);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<Guid> storedTrackIds = await GetOrderedTrackIdsAsync(dbContext, playlistId);
        Assert.Equal([alphaFirstTrackId, alphaSecondTrackId], storedTrackIds);
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenAnAlbumIsScoped_ShouldAppendOnlyThatAlbumsTracks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, "Alpha Artist");
        Guid firstAlbumId = await SeedAlbumAsync(libraryId, artistId, "First Album");
        await SeedTrackAsync(libraryId, firstAlbumId, "First One", 100, trackNumber: 1);
        await SeedTrackAsync(libraryId, firstAlbumId, "First Two", 110, trackNumber: 2);
        Guid secondAlbumId = await SeedAlbumAsync(libraryId, artistId, "Second Album");
        Guid scopedTrackId = await SeedTrackAsync(libraryId, secondAlbumId, "Second One", 120, trackNumber: 1);
        Guid playlistId = await SeedPlaylistAsync(userId, "One album", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(
            libraryId: libraryId, albumId: secondAlbumId, includeAlbumId: true);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries-scope", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistEntriesAddedResponse? addedEntries = await DeserializeAsync<PlaylistEntriesAddedResponse>(response);
        Assert.NotNull(addedEntries);
        Assert.Equal(1, addedEntries!.AddedCount);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<Guid> storedTrackIds = await GetOrderedTrackIdsAsync(dbContext, playlistId);
        Assert.Equal([scopedTrackId], storedTrackIds);
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenTheScopeIsEmpty_ShouldAppendNothingAndReportAZeroCount()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, "Alpha Artist");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "Empty Album");
        Guid playlistId = await SeedPlaylistAsync(userId, "Empty scope", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(
            libraryId: libraryId, albumId: albumId, includeAlbumId: true);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries-scope", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistEntriesAddedResponse? addedEntries = await DeserializeAsync<PlaylistEntriesAddedResponse>(response);
        Assert.NotNull(addedEntries);
        Assert.Equal(0, addedEntries!.AddedCount);
        // An empty scope is a successful no-op, so the playlist activity time is not touched.
        Assert.Null(addedEntries.UpdatedOnUtc);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == playlistId));
        PlaylistEntity playlist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(0, playlist.EntryCount);
        Assert.Equal(0, playlist.Version);
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenThePlaylistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid playlistId = Guid.NewGuid();
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: libraryId);
        string url = $"{PATH}/{playlistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenThePlaylistBelongsToAnotherUser_ShouldReturnNotFoundWithoutAddingEntries()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);
        Guid otherArtistId = await SeedArtistAsync(otherLibraryId, "Someone Else");
        Guid otherAlbumId = await SeedAlbumAsync(otherLibraryId, otherArtistId, "Private Album");
        await SeedTrackAsync(otherLibraryId, otherAlbumId, "Private Track", 200, trackNumber: 1);
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: otherLibraryId);
        string url = $"{PATH}/{otherPlaylistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == otherPlaylistId));
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenTheLibraryBelongsToAnotherUser_ShouldReturnForbiddenWithoutAddingEntries()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Only my library", entryCount: 0);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);
        Guid otherArtistId = await SeedArtistAsync(otherLibraryId, "Not Mine");
        Guid otherAlbumId = await SeedAlbumAsync(otherLibraryId, otherArtistId, "Not Mine Either");
        await SeedTrackAsync(otherLibraryId, otherAlbumId, "Unreachable Track", 210, trackNumber: 1);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: otherLibraryId);
        string url = $"{PATH}/{playlistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == playlistId));
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenTheLibraryDoesNotExist_ShouldReturnForbiddenWithoutAddingEntries()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Unknown library", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: Guid.NewGuid());
        string url = $"{PATH}/{playlistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == playlistId));
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenTheLibraryIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Empty library", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: Guid.Empty);
        string url = $"{PATH}/{playlistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "LibraryIdCannotBeEmpty");
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenTheArtistIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Empty artist", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(
            libraryId: Guid.NewGuid(), artistId: Guid.Empty, includeArtistId: true);
        string url = $"{PATH}/{playlistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "ArtistIdCannotBeEmpty");
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenTheAlbumIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Empty album", entryCount: 0);
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(
            libraryId: Guid.NewGuid(), albumId: Guid.Empty, includeAlbumId: true);
        string url = $"{PATH}/{playlistId}/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "AlbumIdCannotBeEmpty");
    }

    [Fact]
    public async Task AddPlaylistScopeEntries_WhenThePlaylistIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create();
        string url = $"{PATH}/not-a-guid/entries-scope";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
    }

    /// <summary>
    /// Reads the media item Ids of the entries of <paramref name="playlistId"/>, in stored order.
    /// </summary>
    /// <param name="dbContext">The database context to read from.</param>
    /// <param name="playlistId">The Id of the playlist whose entries are read.</param>
    /// <returns>The media item Ids in stored order.</returns>
    private static async Task<List<Guid>> GetOrderedTrackIdsAsync(LuminaDbContext dbContext, Guid playlistId)
    {
        return await dbContext.PlaylistEntries
            .AsNoTracking()
            .Where(candidate => candidate.PlaylistId == playlistId)
            .OrderBy(candidate => candidate.Position)
            .Select(candidate => candidate.MediaItemId)
            .ToListAsync();
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/> and tracks it for cleanup.
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
        dbContext.Albums.Add(_albumEntityFixture.Create(
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
            includeReReleaseYear: false));
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
    /// <param name="trackNumber">The number of the track on its album.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, Guid albumId, string title, int durationInSeconds, int trackNumber)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            title: title,
            trackNumber: trackNumber,
            // Catalogue order is resolved by disc and then track number, so the disc is pinned to keep the appended order deterministic.
            discNumber: 1,
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
    /// Seeds a playlist of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="entryCount">The number of entries the playlist already holds.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name, int entryCount)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid playlistId = Guid.NewGuid();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            type: PlaylistType.Music,
            entryCount: entryCount,
            version: 0,
            createdBy: userId));
        await dbContext.SaveChangesAsync();
        _seededPlaylistIds.Add(playlistId);
        return playlistId;
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
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
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
            dbContext.PlaylistEntries.RemoveRange(dbContext.PlaylistEntries.Where(entry => playlistIds.Contains(entry.PlaylistId)));
            dbContext.Playlists.RemoveRange(dbContext.Playlists.Where(playlist => playlistIds.Contains(playlist.Id)));
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
