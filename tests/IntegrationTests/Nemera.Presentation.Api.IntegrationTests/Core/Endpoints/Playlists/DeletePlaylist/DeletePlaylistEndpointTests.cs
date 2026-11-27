#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.DeletePlaylist;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// Contains integration tests for the <see cref="DeletePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private const string PATH = "/api/v1/users/me/playlists";

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeletePlaylistEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task DeletePlaylist_WhenThePlaylistExists_ShouldDeleteItTogetherWithItsEntries()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "To delete", entryCount: 2);
        await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 0);
        await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 1_000_000_000L);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(content);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        Assert.Empty(document.RootElement.EnumerateObject());

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Playlists.AnyAsync(playlist => playlist.Id == playlistId));
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(entry => entry.PlaylistId == playlistId));
    }

    [Fact]
    public async Task DeletePlaylist_WhenCalledTwice_ShouldDeleteOnlyOnceAndReportTheSecondCallAsNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Deleted once", entryCount: 1);
        Guid otherPlaylistId = await SeedPlaylistAsync(userId, "Untouched", entryCount: 1);
        await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 0);
        await SeedPlaylistEntryAsync(otherPlaylistId, Guid.NewGuid(), 0);

        // Act
        HttpResponseMessage firstResponse = await _client.DeleteAsync($"{PATH}/{playlistId}");
        HttpResponseMessage secondResponse = await _client.DeleteAsync($"{PATH}/{playlistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        await AssertProblemAsync(secondResponse, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", $"{PATH}/{playlistId}");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Playlists.AnyAsync(playlist => playlist.Id == playlistId));
        Assert.True(await dbContext.Playlists.AnyAsync(playlist => playlist.Id == otherPlaylistId));
        Assert.True(await dbContext.PlaylistEntries.AnyAsync(entry => entry.PlaylistId == otherPlaylistId));
    }

    [Fact]
    public async Task DeletePlaylist_WhenThePlaylistBelongsToAnotherUser_ShouldReturnNotFoundAndKeepTheData()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist", entryCount: 1);
        await SeedPlaylistEntryAsync(otherPlaylistId, Guid.NewGuid(), 0);
        string url = $"{PATH}/{otherPlaylistId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Playlists.AnyAsync(playlist => playlist.Id == otherPlaylistId));
        Assert.True(await dbContext.PlaylistEntries.AnyAsync(entry => entry.PlaylistId == otherPlaylistId));
    }

    [Fact]
    public async Task DeletePlaylist_WhenThePlaylistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);
    }

    [Fact]
    public async Task DeletePlaylist_WhenThePlaylistIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string url = $"{PATH}/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
    }

    [Fact]
    public async Task DeletePlaylist_WhenThePlaylistIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        string url = $"{PATH}/{Guid.Empty}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
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
    /// <param name="mediaItemId">The Id of the referenced track.</param>
    /// <param name="position">The sparse order key of the entry.</param>
    private async Task SeedPlaylistEntryAsync(Guid playlistId, Guid mediaItemId, long position)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.PlaylistEntries.Add(_playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: mediaItemId,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: position));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and tracks it for cleanup.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUserIds.Add(userId);
        return userId;
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(candidate => candidate.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Asserts that the response is a problem details document with the expected status, title and detail.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedStatus">The expected HTTP status.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedTitle, string expectedDetail, string expectedInstance)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)expectedStatus, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the response is a validation problem details document carrying the expected error code.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(422, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains(expectedErrorCode, errors!["General.Validation"]);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> playlistIds = [.. _seededPlaylistIds];
            dbContext.PlaylistEntries.RemoveRange(dbContext.PlaylistEntries.Where(entry => playlistIds.Contains(entry.PlaylistId)));
            dbContext.Playlists.RemoveRange(dbContext.Playlists.Where(playlist => playlistIds.Contains(playlist.Id)));
            dbContext.Users.RemoveRange(dbContext.Users.Where(user => _seededUserIds.Contains(user.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
