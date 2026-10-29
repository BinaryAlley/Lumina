#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntry;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Contains security tests for the <see cref="AddPlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/playlists";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly AddPlaylistEntryRequestFixture _addPlaylistEntryRequestFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddPlaylistEntryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        string url = $"{PATH}/{playlistId}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE PlaylistEntries--")] // destructive injection
    [InlineData("1; DELETE FROM Playlists")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task AddPlaylistEntry_WithInjectionInPlaylistId_ShouldRejectWithoutLeakingOrChangingData(string maliciousPlaylistId)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid playlistId = await SeedPlaylistAsync(userId, "Caller playlist", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: Guid.NewGuid());
        string url = $"{PATH}/{Uri.EscapeDataString(maliciousPlaylistId)}/entries";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/", "PlaylistIdCannotBeEmpty");
        Assert.DoesNotContain(maliciousPlaylistId, content, StringComparison.Ordinal);
        Assert.True(await PlaylistExistsAsync(playlistId));
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenThePlaylistBelongsToAnotherUser_ShouldReturnTheSameNotFoundAsAMissingPlaylist()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        Guid ownerPlaylistId = await SeedPlaylistAsync(ownerId, "Owner playlist", entryCount: 0);
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid callerLibraryId = await SeedLibraryAsync(callerId);
        Guid callerTrackId = await SeedTrackAsync(callerLibraryId, "Caller track");
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: callerTrackId);
        Guid missingPlaylistId = Guid.NewGuid();

        // Act
        HttpResponseMessage foreignResponse = await _client.PostAsJsonAsync($"{PATH}/{ownerPlaylistId}/entries", request);
        HttpResponseMessage missingResponse = await _client.PostAsJsonAsync($"{PATH}/{missingPlaylistId}/entries", request);

        // Assert
        // Both answers are indistinguishable, so the route cannot be used as an existence oracle for another user's playlist.
        Assert.Equal(HttpStatusCode.NotFound, foreignResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        string foreignContent = await foreignResponse.Content.ReadAsStringAsync();
        string missingContent = await missingResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(foreignContent), GetProblemDetail(missingContent));
        Assert.Equal("PlaylistNotFound", GetProblemDetail(foreignContent));
        AssertNoSensitiveData(foreignContent);
        Assert.False(await PlaylistHasEntriesAsync(ownerPlaylistId));
        ownerClient.Dispose();
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenTheTrackBelongsToAnotherUsersLibrary_ShouldReturnTheSameNotFoundAsAMissingTrack()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        Guid ownerLibraryId = await SeedLibraryAsync(ownerId);
        Guid ownerTrackId = await SeedTrackAsync(ownerLibraryId, "Unreachable track");
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid callerPlaylistId = await SeedPlaylistAsync(callerId, "Caller playlist", entryCount: 0);
        AddPlaylistEntryRequest foreignTrackRequest = _addPlaylistEntryRequestFixture.Create(trackId: ownerTrackId);
        AddPlaylistEntryRequest missingTrackRequest = _addPlaylistEntryRequestFixture.Create(trackId: Guid.NewGuid());

        // Act
        HttpResponseMessage foreignTrackResponse = await _client.PostAsJsonAsync($"{PATH}/{callerPlaylistId}/entries", foreignTrackRequest);
        HttpResponseMessage missingTrackResponse = await _client.PostAsJsonAsync($"{PATH}/{callerPlaylistId}/entries", missingTrackRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, foreignTrackResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingTrackResponse.StatusCode);
        string foreignTrackContent = await foreignTrackResponse.Content.ReadAsStringAsync();
        string missingTrackContent = await missingTrackResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(foreignTrackContent), GetProblemDetail(missingTrackContent));
        Assert.Equal("TrackNotFound", GetProblemDetail(foreignTrackContent));
        AssertNoSensitiveData(foreignTrackContent);
        Assert.False(await PlaylistHasEntriesAsync(callerPlaylistId));
        ownerClient.Dispose();
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenSuccessful_ShouldReturnOnlyTheCallersDataWithoutSensitiveInformation()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid trackId = await SeedTrackAsync(libraryId, "Caller track");
        Guid playlistId = await SeedPlaylistAsync(userId, "Caller playlist", entryCount: 0);
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"{PATH}/{playlistId}/entries", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        Assert.DoesNotContain("Library-", content, StringComparison.Ordinal);
        Assert.True(await PlaylistHasEntriesAsync(playlistId));
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate.</param>
    /// <returns>The Id and username of the created user.</returns>
    private async Task<(Guid UserId, string Username)> CreateAuthenticatedUserAsync(HttpClient client)
    {
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        return (userId, username);
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
    /// Seeds an artist, an album and a track of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the track belongs to.</param>
    /// <param name="title">The title of the track.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid artistId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(
            id: artistId, libraryId: libraryId, name: $"Artist-{Guid.NewGuid():N}", includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        Guid albumId = Guid.NewGuid();
        dbContext.Albums.Add(_albumEntityFixture.Create(
            id: albumId,
            artistId: artistId,
            libraryId: libraryId,
            title: $"Album-{Guid.NewGuid():N}",
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false));
        await dbContext.SaveChangesAsync();
        Guid trackId = Guid.NewGuid();
        dbContext.Tracks.Add(_trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            title: title,
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false));
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
    /// Asserts that the playlist identified by <paramref name="playlistId"/> still exists.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to look up.</param>
    /// <returns><see langword="true"/> when the playlist still exists.</returns>
    private async Task<bool> PlaylistExistsAsync(Guid playlistId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        return await dbContext.Playlists.AnyAsync(playlist => playlist.Id == playlistId);
    }

    /// <summary>
    /// Determines whether the playlist identified by <paramref name="playlistId"/> holds any entry.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to look up.</param>
    /// <returns><see langword="true"/> when the playlist holds at least one entry.</returns>
    private async Task<bool> PlaylistHasEntriesAsync(Guid playlistId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        return await dbContext.PlaylistEntries.AnyAsync(entry => entry.PlaylistId == playlistId);
    }

    /// <summary>
    /// Reads the detail of a problem details body.
    /// </summary>
    /// <param name="content">The problem details body to read.</param>
    /// <returns>The problem detail.</returns>
    private static string? GetProblemDetail(string content)
    {
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        return problemDetails!["detail"].GetString();
    }

    /// <summary>
    /// Asserts that the response is the unauthenticated problem document produced by the authentication middleware.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private static async Task AssertUnauthorizedAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Asserts the shape of a validation problem details response and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstancePrefix">The expected prefix of the request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private static async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstancePrefix, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.StartsWith(expectedInstancePrefix, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains(expectedErrorCode, errors!["General.Validation"]);
        AssertNoSensitiveData(content);
        return content;
    }

    /// <summary>
    /// Asserts that the response body carries no credentials, secrets or stack traces.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoSensitiveData(string content)
    {
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JWT_SECRET, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Data Source", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asserts that the response body exposes only the fields of the documented playlist entry contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        Assert.DoesNotContain("version", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedBy", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes the catalogue rows and playlists seeded by the test and the test users.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
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
            dbContext.SaveChanges();
        }
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
