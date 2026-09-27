#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteAlbum;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteAlbum;

/// <summary>
/// Contains security tests for the <see cref="DeleteAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteAlbumEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
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
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAlbumEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteAlbumEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task DeleteAlbum_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetProperty("value").GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task DeleteAlbum_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task DeleteAlbum_WithSQLInjectionInArtistId_ShouldNotCorruptOrLeakData(string maliciousArtistId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/albums/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task DeleteAlbum_WithSQLInjectionInAlbumId_ShouldNotCorruptOrLeakData(string maliciousAlbumId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Uri.EscapeDataString(maliciousAlbumId)}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

    }

    [Fact]
    public async Task DeleteAlbum_WhenAlbumBelongsToAnotherUserLibrary_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistWithTwoAlbumsAsync(otherUserId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

    }

    [Fact]
    public async Task DeleteAlbum_WhenAlbumBelongsToAnotherUsersLibrary_ShouldReturnNotFoundWithoutDeletingIt()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        (Guid ownLibraryId, _, _) = await SeedLibraryArtistWithTwoAlbumsAsync(actingUserId);

        // The album lives in a library owned by another user, and is referenced through the acting user's own library route.
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimArtistId, Guid victimAlbumId) = await SeedLibraryArtistWithTwoAlbumsAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{ownLibraryId}/artists/{victimArtistId}/albums/{victimAlbumId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("ArtistNotFound", problemDetails.RootElement.GetProperty("detail").GetString());

        // The other user's album is untouched.
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.NotNull(await dbContext.Albums.FirstOrDefaultAsync(album => album.Id == victimAlbumId));

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
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist that owns two albums.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist and the first album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistWithTwoAlbumsAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid otherAlbumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false, includeBarcode: false, includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false);
        AlbumEntity otherAlbum = _albumEntityFixture.Create(id: otherAlbumId, artistId: artistId, libraryId: libraryId, title: "News of the World", includeTracks: false, includeMetadata: false, includeBarcode: false, includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Reissues", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album, otherAlbum], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId);
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
