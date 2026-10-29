#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.RemovePlaylistEntry;
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

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Contains security tests for the <see cref="RemovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/playlists";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly List<Guid> _seededPlaylistIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RemovePlaylistEntryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH}/{Guid.NewGuid()}/entries/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE PlaylistEntries--")] // destructive injection
    [InlineData("1; DELETE FROM Playlists")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task RemovePlaylistEntry_WithInjectionInPlaylistId_ShouldRejectWithoutLeakingOrDeletingData(string maliciousPlaylistId)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, Guid thirdEntryId) = await SeedOrderedPlaylistAsync(userId);
        string url = $"{PATH}/{Uri.EscapeDataString(maliciousPlaylistId)}/entries/{secondEntryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/", "PlaylistIdCannotBeEmpty");
        Assert.DoesNotContain(maliciousPlaylistId, content, StringComparison.Ordinal);
        Assert.Equal([firstEntryId, secondEntryId, thirdEntryId], await GetOrderedEntryIdsAsync(playlistId));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE PlaylistEntries--")] // destructive injection
    [InlineData("1; DELETE FROM Playlists")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task RemovePlaylistEntry_WithInjectionInPlaylistEntryId_ShouldRejectWithoutLeakingOrDeletingData(string maliciousEntryId)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, Guid thirdEntryId) = await SeedOrderedPlaylistAsync(userId);
        string url = $"{PATH}/{playlistId}/entries/{Uri.EscapeDataString(maliciousEntryId)}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/", "PlaylistEntryIdCannotBeEmpty");
        Assert.DoesNotContain(maliciousEntryId, content, StringComparison.Ordinal);
        Assert.Equal([firstEntryId, secondEntryId, thirdEntryId], await GetOrderedEntryIdsAsync(playlistId));
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenThePlaylistBelongsToAnotherUser_ShouldReturnTheSameNotFoundAsAMissingPlaylist()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        (Guid ownerPlaylistId, Guid ownerFirstEntryId, Guid ownerSecondEntryId, Guid ownerThirdEntryId) = await SeedOrderedPlaylistAsync(ownerId);
        await CreateAuthenticatedUserAsync(_client);
        Guid missingPlaylistId = Guid.NewGuid();

        // Act
        HttpResponseMessage foreignResponse = await _client.DeleteAsync($"{PATH}/{ownerPlaylistId}/entries/{ownerSecondEntryId}");
        HttpResponseMessage missingResponse = await _client.DeleteAsync($"{PATH}/{missingPlaylistId}/entries/{ownerSecondEntryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, foreignResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        string foreignContent = await foreignResponse.Content.ReadAsStringAsync();
        string missingContent = await missingResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(foreignContent), GetProblemDetail(missingContent));
        Assert.Equal("PlaylistNotFound", GetProblemDetail(foreignContent));
        AssertNoSensitiveData(foreignContent);
        Assert.Equal([ownerFirstEntryId, ownerSecondEntryId, ownerThirdEntryId], await GetOrderedEntryIdsAsync(ownerPlaylistId));
        ownerClient.Dispose();
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenTheEntryBelongsToAnotherUsersPlaylist_ShouldReturnTheSameNotFoundAsAMissingEntry()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        (Guid ownerPlaylistId, Guid ownerFirstEntryId, _, _) = await SeedOrderedPlaylistAsync(ownerId);
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid callerPlaylistId, Guid callerFirstEntryId, Guid callerSecondEntryId, Guid callerThirdEntryId) = await SeedOrderedPlaylistAsync(callerId);
        Guid missingEntryId = Guid.NewGuid();

        // Act
        HttpResponseMessage foreignEntryResponse = await _client.DeleteAsync($"{PATH}/{callerPlaylistId}/entries/{ownerFirstEntryId}");
        HttpResponseMessage missingEntryResponse = await _client.DeleteAsync($"{PATH}/{callerPlaylistId}/entries/{missingEntryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, foreignEntryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingEntryResponse.StatusCode);
        string foreignEntryContent = await foreignEntryResponse.Content.ReadAsStringAsync();
        string missingEntryContent = await missingEntryResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(foreignEntryContent), GetProblemDetail(missingEntryContent));
        Assert.Equal("PlaylistEntryNotFound", GetProblemDetail(foreignEntryContent));
        AssertNoSensitiveData(foreignEntryContent);
        Assert.Equal([callerFirstEntryId, callerSecondEntryId, callerThirdEntryId], await GetOrderedEntryIdsAsync(callerPlaylistId));
        Assert.True(await PlaylistExistsAsync(ownerPlaylistId));
        ownerClient.Dispose();
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenSuccessful_ShouldRemoveOnlyTheCallersEntryWithoutSensitiveInformation()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, Guid thirdEntryId) = await SeedOrderedPlaylistAsync(userId);
        string url = $"{PATH}/{playlistId}/entries/{secondEntryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        Assert.Equal([firstEntryId, thirdEntryId], await GetOrderedEntryIdsAsync(playlistId));
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
    /// Seeds a playlist of <paramref name="userId"/> with three ordered entries and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <returns>The Id of the playlist and the Ids of its first, second and third entries.</returns>
    private async Task<(Guid PlaylistId, Guid FirstEntryId, Guid SecondEntryId, Guid ThirdEntryId)> SeedOrderedPlaylistAsync(Guid userId)
    {
        Guid playlistId = await SeedPlaylistAsync(userId, "Ordered playlist", entryCount: 3);
        Guid firstEntryId = await SeedPlaylistEntryAsync(playlistId, 0);
        Guid secondEntryId = await SeedPlaylistEntryAsync(playlistId, 1_000_000_000L);
        Guid thirdEntryId = await SeedPlaylistEntryAsync(playlistId, 2_000_000_000L);
        return (playlistId, firstEntryId, secondEntryId, thirdEntryId);
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="entryCount">The number of entries the playlist holds.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name, int entryCount)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid playlistId = Guid.NewGuid();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            type: PlaylistType.Music,
            entryCount: entryCount,
            version: 0,
            createdBy: userId));
        await dbContext.SaveChangesAsync();
        _seededPlaylistIds.Add(playlistId);
        return playlistId;
    }

    /// <summary>
    /// Seeds an entry of the playlist identified by <paramref name="playlistId"/>.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist the entry belongs to.</param>
    /// <param name="position">The sparse order key of the entry.</param>
    /// <returns>The Id of the seeded entry.</returns>
    private async Task<Guid> SeedPlaylistEntryAsync(Guid playlistId, long position)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid entryId = Guid.NewGuid();
        dbContext.PlaylistEntries.Add(_playlistEntryEntityFixture.Create(
            id: entryId,
            playlistId: playlistId,
            mediaItemId: Guid.NewGuid(),
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: position));
        await dbContext.SaveChangesAsync();
        return entryId;
    }

    /// <summary>
    /// Reads the entry Ids of the playlist identified by <paramref name="playlistId"/>, in stored order.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist whose entries are read.</param>
    /// <returns>The entry Ids in stored order.</returns>
    private async Task<List<Guid>> GetOrderedEntryIdsAsync(Guid playlistId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        return await dbContext.PlaylistEntries
            .AsNoTracking()
            .Where(entry => entry.PlaylistId == playlistId)
            .OrderBy(entry => entry.Position)
            .Select(entry => entry.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Asserts that the playlist identified by <paramref name="playlistId"/> still exists.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to look up.</param>
    /// <returns><see langword="true"/> when the playlist still exists.</returns>
    private async Task<bool> PlaylistExistsAsync(Guid playlistId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        return await dbContext.Playlists.AnyAsync(playlist => playlist.Id == playlistId);
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
    /// Asserts the shape of a validation problem details response and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstancePrefix">The expected prefix of the request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstancePrefix, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.StartsWith(expectedInstancePrefix, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains(expectedErrorCode, errors!["General.Validation"]);
        AssertNoSensitiveData(content);
        return content;
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
    /// Asserts that the response body exposes only the fields of the documented playlist entry contract.
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
