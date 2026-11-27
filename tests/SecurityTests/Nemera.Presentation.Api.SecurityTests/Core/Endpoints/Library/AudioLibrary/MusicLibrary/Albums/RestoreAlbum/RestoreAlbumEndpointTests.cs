#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.RestoreAlbum;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
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

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.RestoreAlbum;

/// <summary>
/// Contains security tests for the <see cref="RestoreAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreAlbumEndpointTests : IClassFixture<NemeraApiFactory>, IDisposable
{
    private readonly NemeraApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly List<string> _seededUsernames = [];
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreAlbumEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreAlbumEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", NemeraApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task RestoreAlbum_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/restore";

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
    [InlineData("'; DROP TABLE Albums; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreAlbum_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreAlbum_WithSQLInjectionInArtistId_ShouldNotCorruptOrLeakData(string maliciousArtistId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/albums/{Guid.NewGuid()}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreAlbum_WithSQLInjectionInAlbumId_ShouldNotCorruptOrLeakData(string maliciousAlbumId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Uri.EscapeDataString(maliciousAlbumId)}/restore", null);

        // Assert
        await AssertRejectedInjectionAttemptAsync(response);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumBelongsToAnotherUser_ShouldReturnForbiddenAndKeepItDeleted()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndDeletedAlbumAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/restore", null);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        AlbumEntity? album = await dbContext.Albums.FirstOrDefaultAsync(album => album.Id == albumId);
        Assert.NotNull(album);
        Assert.True(album!.IsDeleted);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumIsRestoredThroughAnotherLibrary_ShouldReturnNotFoundAndKeepItDeleted()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        Guid ownLibraryId = await SeedLibraryAsync(actingUserId);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimArtistId, Guid victimAlbumId) = await SeedLibraryArtistAndDeletedAlbumAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{ownLibraryId}/artists/{victimArtistId}/albums/{victimAlbumId}/restore", null);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        AlbumEntity? victimAlbum = await dbContext.Albums.FirstOrDefaultAsync(album => album.Id == victimAlbumId);
        Assert.NotNull(victimAlbum);
        Assert.True(victimAlbum!.IsDeleted);
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
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Acting Library", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist and a deleted album of that artist.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist and album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistAndDeletedAlbumAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artistId,
            libraryId: libraryId,
            title: "Victim Album",
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        album.IsDeleted = true;
        album.DeletedOnUtc = DateTime.UtcNow.AddMinutes(-5);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Victim Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Victim Artist", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, album.Id);
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
