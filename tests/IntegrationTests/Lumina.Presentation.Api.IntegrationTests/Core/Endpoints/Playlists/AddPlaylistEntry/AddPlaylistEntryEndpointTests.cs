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
using Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntry;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Contains integration tests for the <see cref="AddPlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly AddPlaylistEntryRequestFixture _addPlaylistEntryRequestFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddPlaylistEntryEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task AddPlaylistEntry_WhenThePlaylistAndTrackExist_ShouldAppendTheEntryAndReturnIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "Innuendo");
        Guid trackId = await SeedTrackAsync(libraryId, albumId, "Innuendo", 393);
        Guid playlistId = await SeedPlaylistAsync(userId, "Evening listening", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        PlaylistEntryAddedResponse? addedEntry = await DeserializeAsync<PlaylistEntryAddedResponse>(response);
        Assert.NotNull(addedEntry);
        Assert.Equal(playlistId, addedEntry!.PlaylistId);
        Assert.NotEqual(default, addedEntry.UpdatedOnUtc);
        PlaylistEntryResponse entry = addedEntry.Entry;
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(trackId, entry.MediaItemId);
        Assert.Equal(PlaylistMediaItemType.MusicTrack, entry.MediaItemType);
        Assert.Equal(0, entry.Position);
        Assert.Equal(libraryId, entry.LibraryId);
        Assert.Equal(albumId, entry.CollectionId);
        Assert.Equal(artistId, entry.CreatorId);
        Assert.Equal("Innuendo", entry.MediaItemTitle);
        Assert.Equal("Innuendo", entry.CollectionTitle);
        Assert.Equal("Queen", entry.CreatorName);
        Assert.Equal(393, entry.DurationInSeconds);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntryEntity? storedEntry = await dbContext.PlaylistEntries.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == entry.Id);
        Assert.NotNull(storedEntry);
        Assert.Equal(playlistId, storedEntry!.PlaylistId);
        Assert.Equal(trackId, storedEntry.MediaItemId);
        Assert.Equal(0, storedEntry.Position);
        PlaylistEntity playlist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(1, playlist.EntryCount);
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenThePlaylistAlreadyHasEntries_ShouldAppendTheEntryAtTheNextDensePosition()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Kind of Magic");
        Guid firstTrackId = await SeedTrackAsync(libraryId, albumId, "One Vision", 308);
        Guid secondTrackId = await SeedTrackAsync(libraryId, albumId, "A Kind of Magic", 262);
        Guid thirdTrackId = await SeedTrackAsync(libraryId, albumId, "Friends Will Be Friends", 248);
        Guid playlistId = await SeedPlaylistAsync(userId, "Ordered listening", entryCount: 2);
        await SeedPlaylistEntryAsync(playlistId, firstTrackId, 0);
        await SeedPlaylistEntryAsync(playlistId, secondTrackId, 1_000_000_000L);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: thirdTrackId);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistEntryAddedResponse? addedEntry = await DeserializeAsync<PlaylistEntryAddedResponse>(response);
        Assert.NotNull(addedEntry);
        // The stored order keys are sparse, so the appended entry reports the dense display position derived from the entry count.
        Assert.Equal(2, addedEntry!.Entry.Position);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<Guid> storedTrackIds = await dbContext.PlaylistEntries
            .AsNoTracking()
            .Where(candidate => candidate.PlaylistId == playlistId)
            .OrderBy(candidate => candidate.Position)
            .Select(candidate => candidate.MediaItemId)
            .ToListAsync();
        Assert.Equal([firstTrackId, secondTrackId, thirdTrackId], storedTrackIds);
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenTheSameTrackIsAddedTwice_ShouldKeepBothEntries()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "News of the World");
        Guid trackId = await SeedTrackAsync(libraryId, albumId, "We Will Rock You", 122);
        Guid playlistId = await SeedPlaylistAsync(userId, "Repeats allowed", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);

        // Act
        HttpResponseMessage firstResponse = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries", request);
        HttpResponseMessage secondResponse = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        PlaylistEntryAddedResponse? firstEntry = await DeserializeAsync<PlaylistEntryAddedResponse>(firstResponse);
        PlaylistEntryAddedResponse? secondEntry = await DeserializeAsync<PlaylistEntryAddedResponse>(secondResponse);
        Assert.NotNull(firstEntry);
        Assert.NotNull(secondEntry);
        Assert.NotEqual(firstEntry!.Entry.Id, secondEntry!.Entry.Id);
        Assert.Equal(0, firstEntry.Entry.Position);
        Assert.Equal(1, secondEntry.Entry.Position);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(2, await dbContext.PlaylistEntries.CountAsync(candidate => candidate.PlaylistId == playlistId && candidate.MediaItemId == trackId));
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenThePlaylistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "Jazz");
        Guid trackId = await SeedTrackAsync(libraryId, albumId, "Bicycle Race", 184);
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);
        string url = $"{PATH}/{playlistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenThePlaylistBelongsToAnotherUser_ShouldReturnNotFoundWithoutAddingAnEntry()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);
        Guid otherArtistId = await SeedArtistAsync(otherLibraryId, "Someone Else");
        Guid otherAlbumId = await SeedAlbumAsync(otherLibraryId, otherArtistId, "Private Album");
        Guid otherTrackId = await SeedTrackAsync(otherLibraryId, otherAlbumId, "Private Track", 200);
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: otherTrackId);
        string url = $"{PATH}/{otherPlaylistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == otherPlaylistId));
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenTheTrackDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Missing track", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: Guid.NewGuid());
        string url = $"{PATH}/{playlistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "TrackNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == playlistId));
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenTheTrackBelongsToAnotherUsersLibrary_ShouldReturnTheSameNotFoundAsAMissingTrack()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Only my library", entryCount: 0);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);
        Guid otherArtistId = await SeedArtistAsync(otherLibraryId, "Not Mine");
        Guid otherAlbumId = await SeedAlbumAsync(otherLibraryId, otherArtistId, "Not Mine Either");
        Guid otherTrackId = await SeedTrackAsync(otherLibraryId, otherAlbumId, "Unreachable Track", 210);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: otherTrackId);
        string url = $"{PATH}/{playlistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        // The unreachable track is reported exactly like a missing one, so the response cannot be used to probe another user's library.
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "TrackNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(candidate => candidate.PlaylistId == playlistId));
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenTheTrackIdIsMissing_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "No track", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(includeTrackId: false);
        string url = $"{PATH}/{playlistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "TrackIdCannotBeEmpty");
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenTheTrackIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Empty track id", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: Guid.Empty);
        string url = $"{PATH}/{playlistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "TrackIdCannotBeEmpty");
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenThePlaylistIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: Guid.NewGuid());
        string url = $"{PATH}/not-a-guid/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
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
    /// Seeds an entry of the playlist identified by <paramref name="playlistId"/>.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist the entry belongs to.</param>
    /// <param name="mediaItemId">The Id of the referenced track.</param>
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
