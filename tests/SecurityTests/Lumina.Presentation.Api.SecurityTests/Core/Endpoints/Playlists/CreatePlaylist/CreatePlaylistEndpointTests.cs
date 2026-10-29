#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.CreatePlaylist;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Contains security tests for the <see cref="CreatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/playlists";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();
    private readonly List<Guid> _seededPlaylistIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CreatePlaylistEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task CreatePlaylist_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertUnauthorizedAsync(response, PATH);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("1; DELETE FROM Playlists")] // stacked statement injection
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task CreatePlaylist_WithInjectionInTheName_ShouldStoreItVerbatimWithoutExecutingOrLeaking(string maliciousName)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: maliciousName, description: maliciousName);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        PlaylistResponse? playlist = JsonSerializer.Deserialize<PlaylistResponse>(content, _jsonOptions);
        Assert.NotNull(playlist);
        _seededPlaylistIds.Add(playlist!.Id);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Equal(maliciousName, storedPlaylist.Name);
        Assert.Equal(maliciousName, storedPlaylist.Description);
        Assert.Equal(userId, storedPlaylist.UserId);
    }

    [Fact]
    public async Task CreatePlaylist_WhenSuccessful_ShouldReturnOnlyTheCallersDataWithoutSensitiveInformation()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: "Caller playlist");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        PlaylistResponse? playlist = JsonSerializer.Deserialize<PlaylistResponse>(content, _jsonOptions);
        Assert.NotNull(playlist);
        _seededPlaylistIds.Add(playlist!.Id);
        Assert.Equal("Caller playlist", playlist.Name);
    }

    [Fact]
    public async Task CreatePlaylist_WhenAnotherUserIsAuthenticated_ShouldNotExposeTheNewPlaylistToThem()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: "Only mine");
        HttpClient otherClient = _apiFactory.CreateClient();
        (_, string otherUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(otherClient);
        _seededUsernames.Add(otherUsername);

        // Act
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(PATH, request);
        string createContent = await createResponse.Content.ReadAsStringAsync();
        PlaylistResponse? playlist = JsonSerializer.Deserialize<PlaylistResponse>(createContent, _jsonOptions);
        Assert.NotNull(playlist);
        _seededPlaylistIds.Add(playlist!.Id);
        HttpResponseMessage ownerResponse = await _client.GetAsync($"{PATH}/{playlist.Id}");
        HttpResponseMessage otherResponse = await otherClient.GetAsync($"{PATH}/{playlist.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        // The other user receives the same not-found answer as for a playlist that never existed, so no data leaks.
        Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);
        string otherContent = await otherResponse.Content.ReadAsStringAsync();
        Assert.Equal("PlaylistNotFound", GetProblemDetail(otherContent));
        Assert.DoesNotContain("Only mine", otherContent, StringComparison.Ordinal);
        AssertNoSensitiveData(otherContent);
        otherClient.Dispose();
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate.</param>
    /// <returns>The Id and username of the created user.</returns>
    private async Task<(Guid UserId, string Username)> CreateAuthenticatedUserAsync(HttpClient client)
    {
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Reads the detail of a problem details body.
    /// </summary>
    /// <param name="content">The problem details body to read.</param>
    /// <returns>The problem detail.</returns>
    private static string? GetProblemDetail(string content)
    {
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        return problemDetails!["detail"].GetString();
    }

    /// <summary>
    /// Asserts that the response is the unauthenticated problem document produced by the authentication middleware.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private static async Task AssertUnauthorizedAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Asserts that the response body carries no credentials, secrets or stack traces.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoSensitiveData(string content)
    {
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JWT_SECRET, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Data Source", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asserts that the response body exposes only the fields of the documented playlist contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        Assert.DoesNotContain("version", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedBy", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes the playlists seeded by the test and the test users.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> playlistIds = [.. _seededPlaylistIds];
            dbContext.PlaylistEntries.RemoveRange(dbContext.PlaylistEntries.Where(entry => playlistIds.Contains(entry.PlaylistId)));
            dbContext.Playlists.RemoveRange(dbContext.Playlists.Where(playlist => playlistIds.Contains(playlist.Id)));
            dbContext.SaveChanges();
        }
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
