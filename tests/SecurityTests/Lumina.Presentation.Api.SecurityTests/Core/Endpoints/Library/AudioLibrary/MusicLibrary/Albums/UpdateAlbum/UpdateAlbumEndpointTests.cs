#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbum;
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

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbum;

/// <summary>
/// Contains security tests for the <see cref="UpdateAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly UpdateAlbumRequestFixture _updateAlbumRequestFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateAlbumEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateAlbum_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(401, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}", problemDetails["instance"].GetProperty("value").GetString());
    }

    [Fact]
    public async Task UpdateAlbum_WhenUnauthorized_ShouldNotLeakSensitiveData()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateAlbum_WithSQLInjectionInBody_ShouldNotCorruptOrLeakData(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: maliciousTitle), contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}", request);

        // Assert
        // The malicious title passes validation, reaches the parameterized update, and is persisted verbatim: if it were
        // concatenated into raw SQL, the update would fail and the album would not be updated.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        AlbumEntity? storedAlbum = await dbContext.Albums.FirstOrDefaultAsync(album => album.Id == albumId);
        Assert.NotNull(storedAlbum);
        Assert.Equal(maliciousTitle, storedAlbum!.Title);
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateAlbum_WithSQLInjectionInLibraryRouteId_ShouldRemainSecure(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousLibraryId, "LibraryIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateAlbum_WithSQLInjectionInArtistRouteId_ShouldRemainSecure(string maliciousArtistId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/albums/{Guid.NewGuid()}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousArtistId, "ArtistIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateAlbum_WithSQLInjectionInAlbumRouteId_ShouldRemainSecure(string maliciousAlbumId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Uri.EscapeDataString(maliciousAlbumId)}", request);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousAlbumId, "AlbumIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateAlbum_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(ownerId);

        HttpClient requesterClient = _apiFactory.CreateClient();
        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(requesterClient);
        _seededUsernames.Add(requesterUsername);
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await requesterClient.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task UpdateAlbum_WhenResourceBelongsToAnotherUsersLibrary_ShouldReturnNotFoundWithoutLeakingOrModifyingIt()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        (Guid ownLibraryId, _, _) = await SeedLibraryArtistAndAlbumAsync(actingUserId);

        // The resource lives in a library owned by another user, and is referenced through the acting user's own library route.
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimArtistId, Guid victimAlbumId) = await SeedLibraryArtistAndAlbumAsync(victimUserId);
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: "Hijacked"), contributors: [], ratings: []);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{ownLibraryId}/artists/{victimArtistId}/albums/{victimAlbumId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("ArtistNotFound", problemDetails.RootElement.GetProperty("detail").GetString());

        // The other user's album is untouched.
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        AlbumEntity? storedAlbum = await dbContext.Albums.FirstOrDefaultAsync(album => album.Id == victimAlbumId);
        Assert.NotNull(storedAlbum);
        Assert.Equal("News of the World", storedAlbum.Title);
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
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist and one of its albums.
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
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "News of the World", includeTracks: false, includeMetadata: false, includeBarcode: false, includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Security Audio Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
