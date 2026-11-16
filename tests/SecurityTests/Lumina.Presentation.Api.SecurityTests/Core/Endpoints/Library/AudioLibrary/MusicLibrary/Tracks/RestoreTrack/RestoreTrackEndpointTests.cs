#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;

/// <summary>
/// Contains security tests for the <see cref="RestoreTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreTrackEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly List<string> _seededUsernames = [];
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreTrackEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task RestoreTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreTrack_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreTrack_WithSQLInjectionInArtistId_ShouldNotCorruptOrLeakData(string maliciousArtistId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreTrack_WithSQLInjectionInAlbumId_ShouldNotCorruptOrLeakData(string maliciousAlbumId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Uri.EscapeDataString(maliciousAlbumId)}/tracks/{Guid.NewGuid()}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreTrack_WithSQLInjectionInTrackId_ShouldNotCorruptOrLeakData(string maliciousTrackId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Uri.EscapeDataString(maliciousTrackId)}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackBelongsToAnotherUser_ShouldReturnForbiddenAndKeepItDeleted()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndDeletedTrackAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore", null);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity? track = await dbContext.Tracks.FirstOrDefaultAsync(track => track.Id == trackId);
        Assert.NotNull(track);
        Assert.True(track!.IsDeleted);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackIsRestoredThroughAnotherLibrary_ShouldReturnNotFoundAndKeepItDeleted()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        Guid ownLibraryId = await SeedLibraryAsync(actingUserId);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimArtistId, Guid victimAlbumId, Guid victimTrackId) = await SeedLibraryArtistAlbumAndDeletedTrackAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{ownLibraryId}/artists/{victimArtistId}/albums/{victimAlbumId}/tracks/{victimTrackId}/restore", null);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity? victimTrack = await dbContext.Tracks.FirstOrDefaultAsync(track => track.Id == victimTrackId);
        Assert.NotNull(victimTrack);
        Assert.True(victimTrack!.IsDeleted);
    }

    /// <summary>
    /// Asserts that a route parameter carrying an injection payload is rejected as a validation error, without leaking internal details.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private async Task AssertRejectedInjectionAttemptAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
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
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Acting Library", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist, an album and a deleted track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndDeletedTrackAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            albumId: albumId,
            libraryId: libraryId,
            path: $"/music/victim/{Guid.NewGuid():N}.flac",
            title: "Victim Track",
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        track.IsDeleted = true;
        track.DeletedOnUtc = DateTime.UtcNow.AddMinutes(-5);
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artistId,
            libraryId: libraryId,
            title: "Victim Album",
            tracks: [track],
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Victim Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Victim Artist", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, album.Id, track.Id);
    }

    /// <summary>
    /// Removes the users created by the tests.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
