#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.AddTrack;

/// <summary>
/// Contains security tests for the <see cref="AddTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly AddTrackRequestFixture _addTrackRequestFixture = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly string _libraryContentLocation = Path.GetTempPath();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddTrackEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task AddTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _addTrackRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks", problemDetails["instance"].GetProperty("value").GetString());
    }

    [Fact]
    public async Task AddTrack_WhenUnauthorized_ShouldNotLeakSensitiveData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddTrackRequest request = _addTrackRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddTrack_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(ownerId);

        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        AddTrackRequest request = _addTrackRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());

        await _apiFactory.RemoveTestUserAsync(ownerUsername);
        await _apiFactory.RemoveTestUserAsync(requesterUsername);
    }

    [Fact]
    public async Task AddTrack_WhenArtistBelongsToAnotherLibrary_ShouldNotDiscloseCrossLibraryData()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        (Guid ownerLibraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(ownerId);

        (Guid attackerId, string attackerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        Guid attackerLibraryId = Guid.NewGuid();
        using (IServiceScope seedScope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext seedDbContext = seedScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            seedDbContext.Libraries.Add(_libraryEntityFixture.Create(id: attackerLibraryId, userId: attackerId, title: "Queen Reissues", libraryType: LibraryType.Music, contentLocations: [_libraryContentLocation]));
            await seedDbContext.SaveChangesAsync();
        }
        AddTrackRequest request = _addTrackRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{attackerLibraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        // The artist exists in another library, but the mismatch is reported as not found, without disclosing that it exists elsewhere.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ArtistNotFound", content, StringComparison.Ordinal);
        Assert.DoesNotContain(ownerLibraryId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);

        await _apiFactory.RemoveTestUserAsync(ownerUsername);
        await _apiFactory.RemoveTestUserAsync(attackerUsername);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddTrack_WithSQLInjectionInPath_ShouldNotCorruptOrDeleteData(string maliciousPath)
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);
        string injectedPath = Path.Combine(_libraryContentLocation, $"{maliciousPath}.flac");
        AddTrackRequest request = _addTrackRequestFixture.Create(path: injectedPath, contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        // The malicious path passes the authenticated handler, reaches the parameterized insert, and is persisted
        // verbatim: if it were concatenated into raw SQL, the insert would fail and the track would not be created.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Libraries.CountAsync(library => library.Id == libraryId));
        TrackEntity storedTrack = await dbContext.Tracks.SingleAsync(track => track.LibraryId == libraryId);
        Assert.Equal(injectedPath, storedTrack.Path);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddTrack_WithSQLInjectionInBody_ShouldNotCorruptOrDeleteData(string maliciousTitle)
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);
        AddTrackRequest request = _addTrackRequestFixture.Create(
            path: Path.Combine(_libraryContentLocation, $"{Guid.NewGuid():N}.flac"),
            metadata: _audioMetadataDtoFixture.Create(title: maliciousTitle),
            contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        // The malicious title passes the authenticated handler, reaches the parameterized insert, and is persisted
        // verbatim: if it were concatenated into raw SQL, the insert would fail and the track would not be created.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity storedTrack = await dbContext.Tracks.SingleAsync(track => track.LibraryId == libraryId);
        Assert.Equal(maliciousTitle, storedTrack.Title);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddTrack_WithSQLInjectionInRouteId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        AddTrackRequest request = _addTrackRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks", request);

        // Assert
        // The route is authoritative and its raw value is kept as a string, so an unparseable route Id reaches the command
        // validator, which reports it as a clean validation error. The response must not leak stack traces, database details,
        // or the injected payload.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("LibraryIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, along with one artist and one album of that library.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist and album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistAndAlbumAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: [_libraryContentLocation]));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        // Nothing to clean up: each test removes its own seeded rows.
    }
}
