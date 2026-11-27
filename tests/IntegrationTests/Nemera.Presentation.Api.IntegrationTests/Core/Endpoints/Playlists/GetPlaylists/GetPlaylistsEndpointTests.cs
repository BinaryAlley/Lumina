#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylists;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaylistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private const string PATH = "/api/v1/users/me/playlists";

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPlaylistsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetPlaylists_WhenTheCallerHasNoPlaylists_ShouldReturnAnEmptyList()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PlaylistSummaryResponse[]? playlists = await DeserializeAsync<PlaylistSummaryResponse[]>(response);
        Assert.NotNull(playlists);
        Assert.Empty(playlists!);
    }

    [Fact]
    public async Task GetPlaylists_WhenTheCallerHasPlaylists_ShouldReturnThemWithTheirEntryCountsOrderedByActivity()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        DateTime now = DateTime.UtcNow;
        Guid oldestId = await SeedPlaylistAsync(userId, "Beta Morning", description: "Oldest", entryCount: 0, createdOnUtc: now.AddHours(-3));
        Guid middleId = await SeedPlaylistAsync(userId, "Alpha Evening", description: "Middle", entryCount: 2, createdOnUtc: now.AddHours(-2));
        Guid mostRecentId = await SeedPlaylistAsync(userId, "Gamma Afternoon", description: "Most recent", entryCount: 1, createdOnUtc: now.AddHours(-4), updatedOnUtc: now);

        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PlaylistSummaryResponse[]? playlists = await DeserializeAsync<PlaylistSummaryResponse[]>(response);
        Assert.NotNull(playlists);
        Assert.Equal(3, playlists!.Length);
        // The order is the most recent activity first: the updated playlist, then the two that were never updated, by creation time.
        Assert.Equal([mostRecentId, middleId, oldestId], playlists.Select(playlist => playlist.Id));

        PlaylistSummaryResponse mostRecent = playlists[0];
        Assert.Equal("Gamma Afternoon", mostRecent.Name);
        Assert.Equal("Most recent", mostRecent.Description);
        Assert.Equal(PlaylistType.Music, mostRecent.Type);
        Assert.Equal(1, mostRecent.EntryCount);
        Assert.NotEqual(default, mostRecent.CreatedOnUtc);
        Assert.NotNull(mostRecent.UpdatedOnUtc);

        PlaylistSummaryResponse middle = playlists[1];
        Assert.Equal("Alpha Evening", middle.Name);
        Assert.Equal(2, middle.EntryCount);
        Assert.Null(middle.UpdatedOnUtc);
    }

    [Fact]
    public async Task GetPlaylists_WhenTwoPlaylistsShareTheSameActivityTime_ShouldOrderThemByName()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        DateTime sharedCreationTime = DateTime.UtcNow.AddHours(-1);
        Guid zebraId = await SeedPlaylistAsync(userId, "Zebra", description: null, entryCount: 0, createdOnUtc: sharedCreationTime);
        Guid alphaId = await SeedPlaylistAsync(userId, "Alpha", description: null, entryCount: 0, createdOnUtc: sharedCreationTime);

        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistSummaryResponse[]? playlists = await DeserializeAsync<PlaylistSummaryResponse[]>(response);
        Assert.NotNull(playlists);
        Assert.Equal([alphaId, zebraId], playlists!.Select(playlist => playlist.Id));
    }

    [Fact]
    public async Task GetPlaylists_WhenAnotherUserHasPlaylists_ShouldReturnOnlyTheCallersPlaylists()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid ownPlaylistId = await SeedPlaylistAsync(userId, "Mine", description: null, entryCount: 0, createdOnUtc: DateTime.UtcNow);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist", description: "Private", entryCount: 3, createdOnUtc: DateTime.UtcNow);

        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Someone else's playlist", content, StringComparison.Ordinal);
        Assert.DoesNotContain(otherPlaylistId.ToString(), content, StringComparison.Ordinal);
        PlaylistSummaryResponse[]? playlists = await DeserializeAsync<PlaylistSummaryResponse[]>(response);
        Assert.NotNull(playlists);
        PlaylistSummaryResponse playlist = Assert.Single(playlists!);
        Assert.Equal(ownPlaylistId, playlist.Id);
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="description">The description of the playlist, or <see langword="null"/>.</param>
    /// <param name="entryCount">The number of entries the playlist holds.</param>
    /// <param name="createdOnUtc">The creation time of the playlist.</param>
    /// <param name="updatedOnUtc">The activity time of the playlist, or <see langword="null"/> when it was never updated.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name, string? description, int entryCount, DateTime createdOnUtc, DateTime? updatedOnUtc = null)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid playlistId = Guid.NewGuid();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            description: description ?? string.Empty,
            includeDescription: description is not null,
            type: PlaylistType.Music,
            entryCount: entryCount,
            version: 0,
            createdBy: userId));
        await dbContext.SaveChangesAsync();
        // The auditing interceptor overwrites both timestamps when a row is inserted, so the activity times that the ordering depends on
        // are written afterwards, in the same form a real details update or entry operation would leave them.
        await dbContext.Playlists
            .Where(playlist => playlist.Id == playlistId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(playlist => playlist.CreatedOnUtc, createdOnUtc)
                .SetProperty(playlist => playlist.UpdatedOnUtc, updatedOnUtc)
                .SetProperty(playlist => playlist.UpdatedBy, updatedOnUtc is null ? (Guid?)null : userId));
        _seededPlaylistIds.Add(playlistId);
        return playlistId;
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
    /// Deserializes the body of <paramref name="response"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the body to.</typeparam>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized body.</returns>
    private async Task<TValue?> DeserializeAsync<TValue>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TValue>(content, _jsonOptions);
    }

    /// <summary>
    /// Asserts that the response carries the cache headers of a private, authorization varying resource.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertCacheHeaders(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
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
