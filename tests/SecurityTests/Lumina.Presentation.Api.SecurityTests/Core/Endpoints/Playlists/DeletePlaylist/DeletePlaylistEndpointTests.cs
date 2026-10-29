#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.DeletePlaylist;
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

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// Contains security tests for the <see cref="DeletePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
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
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeletePlaylistEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task DeletePlaylist_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("1; DELETE FROM Playlists")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task DeletePlaylist_WithInjectionInPlaylistId_ShouldRejectWithoutLeakingOrDeletingData(string maliciousPlaylistId)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid playlistId = await SeedPlaylistAsync(userId, "Caller playlist", entryCount: 0);
        string url = $"{PATH}/{Uri.EscapeDataString(maliciousPlaylistId)}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/", "PlaylistIdCannotBeEmpty");
        Assert.DoesNotContain(maliciousPlaylistId, content, StringComparison.Ordinal);
        Assert.True(await PlaylistExistsAsync(playlistId));
    }

    [Fact]
    public async Task DeletePlaylist_WhenThePlaylistBelongsToAnotherUser_ShouldReturnTheSameNotFoundAsAMissingPlaylist()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        Guid ownerPlaylistId = await SeedPlaylistAsync(ownerId, "Owner playlist", entryCount: 0);
        await CreateAuthenticatedUserAsync(_client);
        Guid missingPlaylistId = Guid.NewGuid();

        // Act
        HttpResponseMessage foreignResponse = await _client.DeleteAsync($"{PATH}/{ownerPlaylistId}");
        HttpResponseMessage missingResponse = await _client.DeleteAsync($"{PATH}/{missingPlaylistId}");

        // Assert
        // Both answers are indistinguishable, so the route cannot be used as an existence oracle for another user's playlist.
        Assert.Equal(HttpStatusCode.NotFound, foreignResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        string foreignContent = await foreignResponse.Content.ReadAsStringAsync();
        string missingContent = await missingResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(foreignContent), GetProblemDetail(missingContent));
        Assert.Equal("PlaylistNotFound", GetProblemDetail(foreignContent));
        AssertNoSensitiveData(foreignContent);
        Assert.True(await PlaylistExistsAsync(ownerPlaylistId));
        ownerClient.Dispose();
    }

    [Fact]
    public async Task DeletePlaylist_WhenSuccessful_ShouldDeleteOnlyTheCallersPlaylistWithoutSensitiveInformation()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid playlistId = await SeedPlaylistAsync(userId, "Caller playlist", entryCount: 1);
        Guid otherPlaylistId = await SeedPlaylistAsync(userId, "Caller other playlist", entryCount: 0);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        Assert.False(await PlaylistExistsAsync(playlistId));
        Assert.True(await PlaylistExistsAsync(otherPlaylistId));
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
        if (entryCount > 0)
        {
            dbContext.PlaylistEntries.Add(_playlistEntryEntityFixture.Create(
                playlistId: playlistId,
                mediaItemId: Guid.NewGuid(),
                mediaItemType: PlaylistMediaItemType.MusicTrack,
                position: 0));
            await dbContext.SaveChangesAsync();
        }
        return playlistId;
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
