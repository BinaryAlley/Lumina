#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.UpdateTrack;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.UpdateTrack;

/// <summary>
/// Contains security tests for the <see cref="UpdateTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private static readonly string s_contentRootPath = Path.Combine(Path.GetTempPath(), "lumina-security-update-track-tests");

    private readonly LuminaApiFactory _apiFactory;
    private readonly UpdateTrackRequestFixture _updateTrackRequestFixture = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateTrackEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(401, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", problemDetails["instance"].GetProperty("value").GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateTrack_WithSQLInjectionInBody_ShouldNotCorruptOrLeakData(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        Guid libraryId = Guid.NewGuid();
        SeedLibrary(libraryId, userId);
        (Guid artistId, Guid albumId, Guid trackId) = SeedArtistGraph(libraryId);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create(
            path: Path.Combine(s_contentRootPath, $"injected-{Guid.NewGuid():N}.flac"),
            metadata: _musicTrackMetadataDtoFixture.Create(title: maliciousTitle),
            moods: [],
            isrcs: [],
            contributors: [],
            ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        // The malicious title passes validation, reaches the parameterized update, and is persisted verbatim: if it were
        // concatenated into raw SQL, the update would fail and the track would not be updated.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity? storedTrack = await dbContext.Tracks.FirstOrDefaultAsync(track => track.Id == trackId);
        Assert.NotNull(storedTrack);
        Assert.Equal(maliciousTitle, storedTrack!.Title);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateTrack_WithSQLInjectionInLibraryRouteId_ShouldRemainSecure(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousLibraryId, "LibraryIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateTrack_WithSQLInjectionInArtistRouteId_ShouldRemainSecure(string maliciousArtistId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousArtistId, "ArtistIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateTrack_WithSQLInjectionInAlbumRouteId_ShouldRemainSecure(string maliciousAlbumId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Uri.EscapeDataString(maliciousAlbumId)}/tracks/{Guid.NewGuid()}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousAlbumId, "AlbumIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateTrack_WithSQLInjectionInTrackRouteId_ShouldRemainSecure(string maliciousTrackId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Uri.EscapeDataString(maliciousTrackId)}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousTrackId, "TrackIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenArtistDoesNotExist_ShouldReturnNotFoundWithoutLeakingInternals()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        Guid libraryId = Guid.NewGuid();
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Hash", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("ArtistNotFound", problemDetails.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateTrack_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        Guid libraryId = Guid.NewGuid();
        SeedLibrary(libraryId, ownerId);
        (Guid artistId, Guid albumId, Guid trackId) = SeedArtistGraph(libraryId);

        HttpClient requesterClient = _apiFactory.CreateClient();
        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(requesterClient);
        _seededUsernames.Add(requesterUsername);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        HttpResponseMessage response = await requesterClient.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task UpdateTrack_WhenTrackBelongsToAnotherUsersLibrary_ShouldReturnNotFoundWithoutLeakingOrModifyingIt()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        Guid ownLibraryId = Guid.NewGuid();
        SeedLibrary(ownLibraryId, actingUserId);

        // The track lives in a library owned by another user, and is referenced through the acting user's own library route.
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        Guid victimLibraryId = Guid.NewGuid();
        SeedLibrary(victimLibraryId, victimUserId);
        (Guid victimArtistId, Guid victimAlbumId, Guid victimTrackId) = SeedArtistGraph(victimLibraryId);
        UpdateTrackRequest request = _updateTrackRequestFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: "Hijacked"));

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{ownLibraryId}/artists/{victimArtistId}/albums/{victimAlbumId}/tracks/{victimTrackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("ArtistNotFound", problemDetails.RootElement.GetProperty("detail").GetString());

        // The other user's track is untouched.
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.NotNull(await dbContext.Tracks.FirstOrDefaultAsync(track => track.Id == victimTrackId));
    }

    /// <summary>
    /// Asserts that the response is a clean validation problem details that neither leaks internals nor echoes the injected payload.
    /// </summary>
    /// <param name="response">The HTTP response to assert.</param>
    /// <param name="injectedPayload">The injected payload that must never be echoed back.</param>
    /// <param name="expectedErrorCode">The validation error code expected in the response.</param>
    private static async Task AssertCleanValidationErrorAsync(HttpResponseMessage response, string injectedPayload, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(injectedPayload, content, StringComparison.Ordinal);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains(expectedErrorCode, content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Seeds a media library of type music owned by <paramref name="userId"/>, whose single content location is the test content root path.
    /// </summary>
    /// <param name="libraryId">The Id of the media library to seed.</param>
    /// <param name="userId">The Id of the user that owns the media library.</param>
    private void SeedLibrary(Guid libraryId, Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Security Audio Library", libraryType: LibraryType.Music, contentLocations: [s_contentRootPath]));
        dbContext.SaveChanges();
        _seededLibraryIds.Add(libraryId);
    }

    /// <summary>
    /// Seeds an artist that owns a single album, which in turn owns a single track, all belonging to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <returns>The Ids of the seeded artist, album and track.</returns>
    private (Guid artistId, Guid albumId, Guid trackId) SeedArtistGraph(Guid libraryId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"), includeMetadata: false);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, libraryId: libraryId, tracks: [track], includeMetadata: false);
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, albums: [album], includeContributors: false);
        dbContext.Artists.Add(artist);
        dbContext.SaveChanges();
        return (artist.Id, albumId, trackId);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<ArtistEntity> artists = [.. dbContext.Artists.Where(artist => _seededLibraryIds.Contains(artist.LibraryId))];
            dbContext.Artists.RemoveRange(artists);
            List<LibraryEntity> libraries = [.. dbContext.Libraries.Where(library => _seededLibraryIds.Contains(library.Id))];
            dbContext.Libraries.RemoveRange(libraries);
            dbContext.SaveChanges();
        }

        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
