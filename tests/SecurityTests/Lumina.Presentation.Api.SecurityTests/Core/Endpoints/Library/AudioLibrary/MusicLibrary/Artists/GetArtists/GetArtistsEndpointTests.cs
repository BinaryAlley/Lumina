#region ========================================================================= USING =====================================================================================
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtists;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtists;

/// <summary>
/// Contains security tests for the <see cref="GetArtistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetArtistsEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetArtists_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists";

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
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetProperty("value").GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetArtists_WhenSearchTermContainsSqlInjection_ShouldNotCorruptOrDeleteData(string maliciousSearchTerm)
    {
        // Arrange
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        Guid libraryId = await SeedLibraryAsync(userId);
        await SeedArtistAsync(libraryId, "Queen");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists?searchTerm={Uri.EscapeDataString(maliciousSearchTerm)}");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousSearchTerm, content, StringComparison.Ordinal);

        // the injected statement must never be executed: the Artists table and the seeded rows must still be there
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Artists.CountAsync(artist => artist.LibraryId == libraryId));

    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task GetArtists_WithSQLInjectionInRouteId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("LibraryIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);

    }

    [Fact]
    public async Task GetArtists_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        Guid libraryId = await SeedLibraryAsync(ownerId);

        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(requesterUsername);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

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
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Reissues", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds an artist belonging to the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedArtistAsync(Guid libraryId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Artists.Add(_artistEntityFixture.Create(libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
