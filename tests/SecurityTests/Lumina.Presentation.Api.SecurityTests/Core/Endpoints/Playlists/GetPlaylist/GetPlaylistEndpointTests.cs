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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Contains security tests for the <see cref="GetPlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/playlists";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
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
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPlaylistEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetPlaylist_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("1; DELETE FROM Playlists")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task GetPlaylist_WithInjectionInPlaylistId_ShouldRejectWithoutLeakingOrChangingData(string maliciousPlaylistId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid playlistId = await SeedPlaylistAsync(callerId, "Caller playlist", trackCount: 1);
        string url = $"{PATH}/{Uri.EscapeDataString(maliciousPlaylistId)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/", "PlaylistIdCannotBeEmpty");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousPlaylistId, content, StringComparison.Ordinal);
        await AssertPlaylistStillExistsAsync(playlistId);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistBelongsToAnotherUser_ShouldReturnTheSameNotFoundAsAMissingPlaylist()
    {
        // Arrange
        (_, _) = await CreateAuthenticatedUserAsync();
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist", trackCount: 1);
        Guid missingPlaylistId = Guid.NewGuid();

        // Act
        HttpResponseMessage existingPlaylistResponse = await _client.GetAsync($"{PATH}/{otherPlaylistId}");
        HttpResponseMessage missingPlaylistResponse = await _client.GetAsync($"{PATH}/{missingPlaylistId}");

        // Assert
        // Both answers are indistinguishable, so the route cannot be used as an existence oracle for another user's playlist.
        Assert.Equal(HttpStatusCode.NotFound, existingPlaylistResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingPlaylistResponse.StatusCode);
        string existingPlaylistContent = await existingPlaylistResponse.Content.ReadAsStringAsync();
        string missingPlaylistContent = await missingPlaylistResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(existingPlaylistContent), GetProblemDetail(missingPlaylistContent));
        Assert.Equal("PlaylistNotFound", GetProblemDetail(existingPlaylistContent));
        Assert.DoesNotContain("Someone else's playlist", existingPlaylistContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPlaylist_WhenSuccessful_ShouldReturnOnlyTheCallersPlaylistWithoutSensitiveData()
    {
        // Arrange
        (Guid callerId, string username) = await CreateAuthenticatedUserAsync();
        Guid playlistId = await SeedPlaylistAsync(callerId, "Caller playlist", trackCount: 2);
        (Guid otherUserId, string otherUsername) = await SeedOtherUserAsync();
        await SeedPlaylistAsync(otherUserId, "Other playlist", trackCount: 1);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        Assert.DoesNotContain(username, content, StringComparison.Ordinal);
        Assert.DoesNotContain(otherUsername, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Other playlist", content, StringComparison.Ordinal);

        PlaylistResponse? playlist = JsonSerializer.Deserialize<PlaylistResponse>(content, _jsonOptions);
        Assert.NotNull(playlist);
        Assert.Equal(playlistId, playlist!.Id);
        Assert.Equal("Caller playlist", playlist.Name);
        Assert.Equal(2, playlist.Entries.Data.Count);
    }

    [Fact]
    public async Task GetPlaylist_WhenThePlaylistDoesNotExist_ShouldRejectWithNonCacheableNotFound()
    {
        // Arrange
        await CreateAuthenticatedUserAsync();
        string url = $"{PATH}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertNotFoundAsync(response, url, "PlaylistNotFound");
        AssertNonCacheable(response);
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <returns>The Id and username of the created user.</returns>
    private async Task<(Guid UserId, string Username)> CreateAuthenticatedUserAsync()
    {
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id and username.
    /// </summary>
    /// <returns>The Id and username of the seeded user.</returns>
    private async Task<(Guid UserId, string Username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> with the requested number of entries, each one referencing its own seeded track.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="trackCount">The number of entries to seed.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name, int trackCount)
    {
        Guid libraryId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId, userId: userId, title: $"Library-{Guid.NewGuid():N}", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        Guid artistId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Playlist Artist", includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        Guid albumId = Guid.NewGuid();
        dbContext.Albums.Add(_albumEntityFixture.Create(
            id: albumId,
            artistId: artistId,
            libraryId: libraryId,
            title: "Playlist Album",
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false));
        await dbContext.SaveChangesAsync();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            type: PlaylistType.Music,
            entryCount: trackCount,
            version: 0,
            createdBy: userId));
        await dbContext.SaveChangesAsync();
        _seededPlaylistIds.Add(playlistId);
        for (int index = 0; index < trackCount; index++)
        {
            Guid trackId = Guid.NewGuid();
            dbContext.Tracks.Add(_trackEntityFixture.Create(
                id: trackId,
                albumId: albumId,
                libraryId: libraryId,
                title: $"Playlist Track {index}",
                includeMetadata: false,
                includeOriginalReleaseDate: false,
                includeOriginalReleaseYear: false,
                includeReReleaseDate: false,
                includeReReleaseYear: false));
            await dbContext.SaveChangesAsync();
            dbContext.PlaylistEntries.Add(_playlistEntryEntityFixture.Create(
                playlistId: playlistId,
                mediaItemId: trackId,
                mediaItemType: PlaylistMediaItemType.MusicTrack,
                position: index * 1_000_000_000L));
            await dbContext.SaveChangesAsync();
        }
        return playlistId;
    }

    /// <summary>
    /// Asserts that the playlist identified by <paramref name="playlistId"/> still exists.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to look up.</param>
    private async Task AssertPlaylistStillExistsAsync(Guid playlistId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Playlists.AnyAsync(playlist => playlist.Id == playlistId));
    }

    /// <summary>
    /// Asserts that the response is the unauthenticated problem document produced by the authentication middleware.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertUnauthorizedAsync(HttpResponseMessage response, string expectedInstance)
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
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstancePrefix, string expectedErrorCode)
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
        // The instance is the routed path, which normalizes the escaping of some characters, so only its prefix is asserted.
        Assert.StartsWith(expectedInstancePrefix, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorCode, errors["General.Validation"]);
        AssertNoSensitiveData(content);
        return content;
    }

    /// <summary>
    /// Asserts the shape of a not found problem details response and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertNotFoundAsync(HttpResponseMessage response, string expectedInstance, string expectedDetail)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        AssertNoSensitiveData(content);
        return content;
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
    /// Asserts that the response is not storable by any intermediary and varies by the authorization header.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertNonCacheable(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
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
    /// Asserts that the response body exposes only the fields of the documented playlist response contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        // The creation and update times are part of the documented playlist contract, so they are not asserted absent here.
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
            dbContext.Libraries.RemoveRange(dbContext.Libraries.Where(library => libraryIds.Contains(library.Id)));
            dbContext.SaveChanges();
        }
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
