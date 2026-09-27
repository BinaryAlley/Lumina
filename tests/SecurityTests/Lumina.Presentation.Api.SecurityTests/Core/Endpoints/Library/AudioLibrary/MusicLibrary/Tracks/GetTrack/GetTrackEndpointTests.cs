#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;

/// <summary>
/// Contains security tests for the <see cref="GetTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTrackEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task GetTrack_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}");

        // Assert
        // The route value is kept as a raw string, so the unparseable Id reaches the query validator, which reports a clean validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);
        Assert.DoesNotContain("testuser_", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task GetTrack_WithSQLInjectionInArtistId_ShouldNotCorruptOrLeakData(string maliciousArtistId)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}");

        // Assert
        // The route value is kept as a raw string, so the unparseable Id reaches the query validator, which reports a clean validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousArtistId, content, StringComparison.Ordinal);
        Assert.DoesNotContain("testuser_", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task GetTrack_WithSQLInjectionInAlbumId_ShouldNotCorruptOrLeakData(string maliciousAlbumId)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Uri.EscapeDataString(maliciousAlbumId)}/tracks/{Guid.NewGuid()}");

        // Assert
        // The route value is kept as a raw string, so the unparseable Id reaches the query validator, which reports a clean
        // validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousAlbumId, content, StringComparison.Ordinal);
        Assert.DoesNotContain("testuser_", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task GetTrack_WithSQLInjectionInTrackId_ShouldNotCorruptOrLeakData(string maliciousTrackId)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Uri.EscapeDataString(maliciousTrackId)}");

        // Assert
        // The route value is kept as a raw string, so the unparseable Id reaches the query validator, which reports a clean validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousTrackId, content, StringComparison.Ordinal);
        Assert.DoesNotContain("testuser_", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Fact]
    public async Task GetTrack_WhenTrackBelongsToAnotherUserLibrary_ShouldReturnForbiddenResult()
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded other user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return userId;
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Reissues", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false));
        AlbumEntity album = _albumEntityFixture.Create(
            id: albumId,
            artistId: artistId,
            libraryId: libraryId,
            title: "News of the World",
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            path: $"/music/queen/we-are-the-champions-{Guid.NewGuid():N}.flac",
            title: "We Are the Champions",
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        dbContext.Albums.Add(album);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId, trackId);
    }

    /// <summary>
    /// Disposes the API factory resources.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
