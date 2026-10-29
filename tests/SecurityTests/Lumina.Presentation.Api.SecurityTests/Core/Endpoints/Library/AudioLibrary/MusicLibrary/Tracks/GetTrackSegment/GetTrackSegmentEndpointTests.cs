#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;

/// <summary>
/// Contains security tests for the <see cref="GetTrackSegmentEndpoint"/> class.
/// </summary>
/// <remarks>
/// The concurrency rate limiter of the segment route is not exercised, because it requires four concurrent long lived streams, which needs a real
/// normalization pipeline and ffmpeg, neither of which is available in the test environment.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const int DEFAULT_SEGMENT_INDEX = 0;
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly string _contentRootPath = Path.Combine(Path.GetTempPath(), $"lumina-track-segment-security-tests-{Guid.NewGuid():N}");
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTrackSegmentEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        Directory.CreateDirectory(_contentRootPath);
    }

    [Fact]
    public async Task GetTrackSegment_WhenCalledWithoutCredentials_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString("N"), 0));
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
    [InlineData("deliveryId", "'; DROP TABLE Tracks; --")] // destructive injection in the delivery handle
    [InlineData("deliveryId", "' OR '1'='1")] // boolean based injection in the delivery handle
    [InlineData("segmentIndex", "'; DROP TABLE Tracks; --")] // destructive injection in the segment index
    [InlineData("segmentIndex", "' OR '1'='1")] // boolean based injection in the segment index
    [InlineData("libraryId", "../../etc/passwd")] // path traversal in the library Id
    [InlineData("artistId", "../../etc/passwd")] // path traversal in the artist Id
    [InlineData("albumId", "../../etc/passwd")] // path traversal in the album Id
    [InlineData("trackId", "../../etc/passwd")] // path traversal in the track Id
    [InlineData("deliveryId", "../../etc/passwd")] // path traversal in the delivery handle
    [InlineData("segmentIndex", "../../etc/passwd")] // path traversal in the segment index
    public async Task GetTrackSegment_WithAnInjectionPayloadInARouteValue_ShouldNotFailOrLeakData(string routeValue, string payload)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string path = BuildSegmentPathWithPayload(routeValue, payload);

        // Act
        HttpResponseMessage response = await _client.GetAsync(path);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("/etc/passwd", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrackSegment_WhenDeliveryIdIsWellFormedButUnknown_ShouldReturnGenericNotFound()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        // A library owned by the authenticated user keeps the ownership check passing, so the unknown handle is what rejects the request.
        Guid libraryId = await SeedLibraryAsync(userId);
        string unknownDeliveryId = Guid.NewGuid().ToString("N");

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(libraryId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), unknownDeliveryId, 0));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("PlaybackDeliveryNotFound", problemDetails.Detail);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrackSegment_WhenLibraryBelongsToAnotherUser_ShouldReturnForbiddenWithoutLeakingTheTrackPath()
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        Guid otherUserId = await SeedOtherUserAsync();
        string secretTrackPath = Path.Combine(_contentRootPath, $"secret-{Guid.NewGuid():N}.flac");
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, secretTrackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(libraryId, artistId, albumId, trackId, Guid.NewGuid().ToString("N"), 0));
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
    public async Task GetTrackSegment_WhenManifestHandleIsUsedForAnotherTrack_ShouldReturnNotFound()
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        string trackPath = Path.Combine(_contentRootPath, $"delivered-{Guid.NewGuid():N}.ogg");
        await File.WriteAllBytesAsync(trackPath, [1, 2, 3, 4, 5, 6, 7, 8]);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        string deliveryId = await GetDeliveryIdAsync(libraryId, artistId, albumId, trackId);
        Guid otherTrackId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(libraryId, artistId, albumId, otherTrackId, deliveryId, 0));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("PlaybackDeliveryNotFound", problemDetails.Detail);
        Assert.DoesNotContain(trackPath, content, StringComparison.OrdinalIgnoreCase);
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
    /// Seeds a music library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
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
        dbContext.Tracks.Add(_trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            path: trackPath,
            container: "OGG",
            audioCodec: "OPUS",
            title: "A Track",
            includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId, trackId);
    }

    /// <summary>
    /// Requests a transcoded playback manifest for the seeded track and returns the delivery handle it reports.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The delivery handle of the normalized delivery.</returns>
    private async Task<string> GetDeliveryIdAsync(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest?capabilities=mp3/mp3");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        PlaybackManifestResponse? manifest = JsonSerializer.Deserialize<PlaybackManifestResponse>(content, _jsonOptions);
        Assert.NotNull(manifest);
        Assert.NotNull(manifest!.DeliveryId);
        return manifest.DeliveryId;
    }

    /// <summary>
    /// Builds the route of a single segment of the normalized audio of a track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <param name="deliveryId">The opaque handle of the normalized delivery.</param>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <returns>The route of the requested segment.</returns>
    private static string BuildSegmentPath(Guid libraryId, Guid artistId, Guid albumId, Guid trackId, string deliveryId, int segmentIndex)
    {
        return $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream/deliveries/{deliveryId}/segments/{segmentIndex}";
    }

    /// <summary>
    /// Builds the route of a single segment of the normalized audio of a track, with a malicious payload in one of its route values.
    /// </summary>
    /// <param name="routeValue">The name of the route value that carries the payload.</param>
    /// <param name="payload">The malicious payload.</param>
    /// <returns>The route of the requested segment, with the payload URL-escaped.</returns>
    private static string BuildSegmentPathWithPayload(string routeValue, string payload)
    {
        string libraryId = routeValue == "libraryId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string artistId = routeValue == "artistId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string albumId = routeValue == "albumId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string trackId = routeValue == "trackId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString();
        string deliveryId = routeValue == "deliveryId" ? Uri.EscapeDataString(payload) : Guid.NewGuid().ToString("N");
        string segmentIndex = routeValue == "segmentIndex" ? Uri.EscapeDataString(payload) : DEFAULT_SEGMENT_INDEX.ToString();
        return $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream/deliveries/{deliveryId}/segments/{segmentIndex}";
    }

    /// <summary>
    /// Disposes the API factory resources, the seeded users and the temporary files of the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, recursive: true);
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
