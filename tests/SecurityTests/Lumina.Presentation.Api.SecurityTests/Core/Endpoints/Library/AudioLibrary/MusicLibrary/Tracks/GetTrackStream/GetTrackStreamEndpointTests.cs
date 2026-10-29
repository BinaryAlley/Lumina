#region ========================================================================= USING =====================================================================================
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;

/// <summary>
/// Contains security tests for the <see cref="GetTrackStreamEndpoint"/> class.
/// </summary>
/// <remarks>
/// The concurrency rate limiter of the stream route is not exercised, because it requires four concurrent long lived streams, which needs a real
/// normalization pipeline and ffmpeg, neither of which is available in the test environment.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetTrackStreamEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string ACTIVE_CONTENT_BODY = "<html><body><script>alert('active')</script></body></html>";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly string _contentRootPath = Path.Combine(Path.GetTempPath(), $"lumina-track-stream-security-tests-{Guid.NewGuid():N}");
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _temporaryFiles = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackStreamEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTrackStreamEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        Directory.CreateDirectory(_contentRootPath);
    }

    [Fact]
    public async Task GetTrackStream_WhenCalledWithoutCredentials_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("Unauthorized", problemDetails!["title"].GetString());
    }

    [Theory]
    [InlineData("libraryId", "'; DROP TABLE Tracks; --")] // destructive injection in the library Id
    [InlineData("libraryId", "' OR '1'='1")] // boolean based injection in the library Id
    [InlineData("artistId", "'; DROP TABLE Tracks; --")] // destructive injection in the artist Id
    [InlineData("artistId", "'; EXEC xp_cmdshell('dir'); --")] // command execution attempt in the artist Id
    [InlineData("albumId", "' OR '1'='1")] // boolean based injection in the album Id
    [InlineData("albumId", "'; DROP TABLE Tracks; --")] // destructive injection in the album Id
    [InlineData("trackId", "' OR '1'='1")] // boolean based injection in the track Id
    [InlineData("trackId", "'; EXEC xp_cmdshell('dir'); --")] // command execution attempt in the track Id
    [InlineData("libraryId", "../../etc/passwd")] // path traversal in the library Id
    [InlineData("artistId", "../../etc/passwd")] // path traversal in the artist Id
    [InlineData("albumId", "../../etc/passwd")] // path traversal in the album Id
    [InlineData("trackId", "../../etc/passwd")] // path traversal in the track Id
    public async Task GetTrackStream_WithAnInjectionPayloadInARouteValue_ShouldRejectItWithoutLeakingData(string routeValue, string payload)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string path = BuildStreamPathWithPayload(routeValue, payload);

        // Act
        HttpResponseMessage response = await _client.GetAsync(path);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("/etc/passwd", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrackStream_WhenLibraryBelongsToAnotherUser_ShouldReturnForbiddenWithoutLeakingTheTrackPath()
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        Guid otherUserId = await SeedOtherUserAsync();
        string secretTrackPath = WriteContentFile($"secret-{Guid.NewGuid():N}.flac", [1, 2, 3, 4]);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, secretTrackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(libraryId, artistId, albumId, trackId));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("secret-", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(secretTrackPath, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.Unauthorized", problemDetails!["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task GetTrackStream_WhenTrackIsRequestedThroughAnotherAlbum_ShouldReturnTrackNotFound()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string trackPath = WriteContentFile($"track-{Guid.NewGuid():N}.mp3", [1, 2, 3, 4]);
        (Guid libraryId, Guid artistId, _, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        Guid otherAlbumId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(libraryId, artistId, otherAlbumId, trackId));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(trackPath, content, StringComparison.OrdinalIgnoreCase);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenTrackIsRequestedThroughAnotherArtist_ShouldReturnTrackNotFound()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string trackPath = WriteContentFile($"track-{Guid.NewGuid():N}.mp3", [1, 2, 3, 4]);
        (Guid libraryId, _, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        Guid otherArtistId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(libraryId, otherArtistId, albumId, trackId));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenTrackIsRequestedThroughAnotherLibraryOwnedByTheSameUser_ShouldReturnTrackNotFound()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string trackPath = WriteContentFile($"track-{Guid.NewGuid():N}.mp3", [1, 2, 3, 4]);
        (_, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        Guid otherLibraryId = await SeedEmptyLibraryAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(otherLibraryId, artistId, albumId, trackId));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("TrackNotFound", problemDetails!.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenAudioFileIsOutsideTheLibraryContentLocations_ShouldReturnTrackFileNotFoundWithoutLeakingThePath()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string outsideFilePath = Path.Combine(Path.GetTempPath(), $"lumina-track-stream-outside-{Guid.NewGuid():N}.mp3");
        await File.WriteAllBytesAsync(outsideFilePath, [1, 2, 3, 4]);
        _temporaryFiles.Add(outsideFilePath);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, outsideFilePath);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(libraryId, artistId, albumId, trackId));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(outsideFilePath, content, StringComparison.OrdinalIgnoreCase);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackFileNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenTheAudioFileHasAnActiveContentExtension_ShouldServeItAsOpaqueContentWithSniffingDisabled()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string activeContentPath = WriteContentFile($"active-{Guid.NewGuid():N}.html", System.Text.Encoding.UTF8.GetBytes(ACTIVE_CONTENT_BODY));
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, activeContentPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(libraryId, artistId, albumId, trackId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/octet-stream", response.Content.Headers.ContentType?.MediaType);
        Assert.NotEqual("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("nosniff", response.Headers.GetValues("X-Content-Type-Options"));
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        Assert.Null(response.Content.Headers.ContentDisposition);
    }

    [Fact]
    public async Task GetTrackStream_WhenTheTrackBelongsToAnOwnedLibrary_ShouldServeItPrivatelyWithoutADownloadName()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string trackPath = WriteContentFile($"owned-{Guid.NewGuid():N}.mp3", [1, 2, 3, 4, 5, 6, 7, 8]);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildStreamPath(libraryId, artistId, albumId, trackId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(response.Headers.ETag);
        Assert.NotNull(response.Content.Headers.LastModified);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.DoesNotContain("public", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        Assert.Null(response.Content.Headers.ContentDisposition);
        Assert.DoesNotContain(trackPath, response.Headers.ETag!.Tag.ToString(), StringComparison.OrdinalIgnoreCase);
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
    /// Seeds an empty music library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedEmptyLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Other Library", libraryType: LibraryType.Music, contentLocations: [_contentRootPath]));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="trackPath">The file system path of the audio file of the track.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId, string trackPath)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: [_contentRootPath]));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: trackPath, title: "A Track", includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId, trackId);
    }

    /// <summary>
    /// Writes a file with the provided <paramref name="contentBytes"/> inside the content root of the test, and returns its full path.
    /// </summary>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="contentBytes">The bytes of the file to write.</param>
    /// <returns>The full path of the written file.</returns>
    private string WriteContentFile(string fileName, byte[] contentBytes)
    {
        string filePath = Path.Combine(_contentRootPath, fileName);
        File.WriteAllBytes(filePath, contentBytes);
        return filePath;
    }

    /// <summary>
    /// Builds the route of the direct stream of a track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The route of the requested stream.</returns>
    private static string BuildStreamPath(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream";
    }

    /// <summary>
    /// Builds the route of the direct stream of a track, with a malicious payload in one of its route values.
    /// </summary>
    /// <param name="routeValue">The name of the route value that carries the payload.</param>
    /// <param name="payload">The malicious payload.</param>
    /// <returns>The route of the requested stream, with the payload URL-escaped.</returns>
    private static string BuildStreamPathWithPayload(string routeValue, string payload)
    {
        string libraryId = routeValue == "libraryId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string artistId = routeValue == "artistId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string albumId = routeValue == "albumId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string trackId = routeValue == "trackId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        return $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream";
    }

    /// <summary>
    /// Disposes the API factory resources, the seeded users and the temporary files of the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, recursive: true);
        foreach (string temporaryFile in _temporaryFiles)
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
