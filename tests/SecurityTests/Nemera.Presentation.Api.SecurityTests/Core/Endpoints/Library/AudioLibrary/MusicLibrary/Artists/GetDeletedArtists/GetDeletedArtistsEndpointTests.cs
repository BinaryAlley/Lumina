#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetDeletedArtists;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetDeletedArtists;

/// <summary>
/// Contains security tests for the <see cref="GetDeletedArtistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedArtistsEndpointTests : IClassFixture<NemeraApiFactory>, IDisposable
{
    private readonly NemeraApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly List<string> _seededUsernames = [];
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedArtistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDeletedArtistsEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", NemeraApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetDeletedArtists_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/deleted-items";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task GetDeletedArtists_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetDeletedArtists_WhenLibraryBelongsToAnotherUser_ShouldReturnForbiddenAndNotLeakTheDeletedArtist()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        const string NAME = "Secret Deleted Artist";
        const string COVER_PATH = "/private/deleted-artist-cover.jpg";
        Guid libraryId = await SeedLibraryAndDeletedArtistAsync(victimUserId, NAME, COVER_PATH);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/libraries/{libraryId}/artists/deleted-items");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain(NAME, content, StringComparison.Ordinal);
        Assert.DoesNotContain(COVER_PATH, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with a deleted artist and its cover.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="name">The name of the deleted artist.</param>
    /// <param name="coverPath">The stored cover path of the deleted artist.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAndDeletedArtistAsync(Guid userId, string name, string coverPath)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false);
        artist.IsDeleted = true;
        artist.DeletedOnUtc = DateTime.UtcNow.AddMinutes(-5);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Victim Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(artist);
        dbContext.MusicArtwork.Add(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artist.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: coverPath));
        await dbContext.SaveChangesAsync();
        return libraryId;
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
