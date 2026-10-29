#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.RemovePlaylistEntry;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Contains integration tests for the <see cref="RemovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RemovePlaylistEntryEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task RemovePlaylistEntry_WhenTheEntryExists_ShouldRemoveOnlyThatEntryAndReturnTheResult()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, Guid thirdEntryId) = await SeedOrderedPlaylistAsync(userId);
        string url = $"{PATH}/{playlistId}/entries/{secondEntryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        PlaylistEntryRemovedResponse? removedEntry = await DeserializeAsync<PlaylistEntryRemovedResponse>(response);
        Assert.NotNull(removedEntry);
        Assert.Equal(playlistId, removedEntry!.PlaylistId);
        Assert.Equal(secondEntryId, removedEntry.RemovedEntryId);
        Assert.Equal(0, removedEntry.RepositionedCount);
        Assert.NotEqual(default, removedEntry.UpdatedOnUtc);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(entry => entry.Id == secondEntryId));
        List<Guid> orderedEntryIds = await GetOrderedEntryIdsAsync(dbContext, playlistId);
        Assert.Equal([firstEntryId, thirdEntryId], orderedEntryIds);
        PlaylistEntity playlist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(2, playlist.EntryCount);
        Assert.Equal(1, playlist.Version);
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenTheEntryIsTheLastOne_ShouldLeaveThePlaylistEmpty()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "One entry", entryCount: 1);
        Guid entryId = await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 0);
        string url = $"{PATH}/{playlistId}/entries/{entryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(entry => entry.PlaylistId == playlistId));
        PlaylistEntity playlist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(0, playlist.EntryCount);
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenCalledTwice_ShouldRemoveOnceAndReportTheSecondCallAsNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, _) = await SeedOrderedPlaylistAsync(userId);
        string url = $"{PATH}/{playlistId}/entries/{secondEntryId}";

        // Act
        HttpResponseMessage firstResponse = await _client.DeleteAsync(url);
        HttpResponseMessage secondResponse = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        await AssertProblemAsync(secondResponse, HttpStatusCode.NotFound, "General.NotFound", "PlaylistEntryNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity playlist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(2, playlist.EntryCount);
        Assert.True(await dbContext.PlaylistEntries.AnyAsync(entry => entry.Id == firstEntryId));
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenThePlaylistBelongsToAnotherUser_ShouldReturnNotFoundAndKeepTheEntry()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherPlaylistId, _, Guid otherEntryId, _) = await SeedOrderedPlaylistAsync(otherUserId);
        string url = $"{PATH}/{otherPlaylistId}/entries/{otherEntryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.PlaylistEntries.AnyAsync(entry => entry.Id == otherEntryId));
        PlaylistEntity otherPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == otherPlaylistId);
        Assert.Equal(3, otherPlaylist.EntryCount);
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenTheEntryBelongsToAnotherPlaylist_ShouldReturnNotFoundAndKeepIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, Guid thirdEntryId) = await SeedOrderedPlaylistAsync(userId);
        Guid otherPlaylistId = await SeedPlaylistAsync(userId, "Another playlist", entryCount: 1);
        Guid otherPlaylistEntryId = await SeedPlaylistEntryAsync(otherPlaylistId, Guid.NewGuid(), 0);
        string url = $"{PATH}/{playlistId}/entries/{otherPlaylistEntryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistEntryNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.PlaylistEntries.AnyAsync(entry => entry.Id == otherPlaylistEntryId));
        List<Guid> orderedEntryIds = await GetOrderedEntryIdsAsync(dbContext, playlistId);
        Assert.Equal([firstEntryId, secondEntryId, thirdEntryId], orderedEntryIds);
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenTheEntryBelongsToAnotherUsersPlaylist_ShouldReturnNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid playlistId, Guid firstEntryId, Guid secondEntryId, Guid thirdEntryId) = await SeedOrderedPlaylistAsync(userId);
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherPlaylistId, _, Guid otherEntryId, _) = await SeedOrderedPlaylistAsync(otherUserId);
        string url = $"{PATH}/{playlistId}/entries/{otherEntryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistEntryNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.PlaylistEntries.AnyAsync(entry => entry.Id == otherEntryId));
        List<Guid> orderedEntryIds = await GetOrderedEntryIdsAsync(dbContext, playlistId);
        Assert.Equal([firstEntryId, secondEntryId, thirdEntryId], orderedEntryIds);
        Assert.True(await dbContext.Playlists.AnyAsync(playlist => playlist.Id == otherPlaylistId));
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenThePlaylistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        string url = $"{PATH}/{playlistId}/entries/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenThePlaylistIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string url = $"{PATH}/not-a-guid/entries/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenTheEntryIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Invalid entry id", entryCount: 0);
        string url = $"{PATH}/{playlistId}/entries/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistEntryIdCannotBeEmpty");
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> with three ordered entries and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <returns>The Id of the playlist and the Ids of its first, second and third entries.</returns>
    private async Task<(Guid PlaylistId, Guid FirstEntryId, Guid SecondEntryId, Guid ThirdEntryId)> SeedOrderedPlaylistAsync(Guid userId)
    {
        Guid playlistId = await SeedPlaylistAsync(userId, "Ordered playlist", entryCount: 3);
        Guid firstEntryId = await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 0);
        Guid secondEntryId = await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 1_000_000_000L);
        Guid thirdEntryId = await SeedPlaylistEntryAsync(playlistId, Guid.NewGuid(), 2_000_000_000L);
        return (playlistId, firstEntryId, secondEntryId, thirdEntryId);
    }

    /// <summary>
    /// Reads the entry Ids of the playlist identified by <paramref name="playlistId"/>, in stored order.
    /// </summary>
    /// <param name="dbContext">The database context to read from.</param>
    /// <param name="playlistId">The Id of the playlist whose entries are read.</param>
    /// <returns>The entry Ids in stored order.</returns>
    private static async Task<List<Guid>> GetOrderedEntryIdsAsync(LuminaDbContext dbContext, Guid playlistId)
    {
        return await dbContext.PlaylistEntries
            .AsNoTracking()
            .Where(candidate => candidate.PlaylistId == playlistId)
            .OrderBy(candidate => candidate.Position)
            .Select(candidate => candidate.Id)
            .ToListAsync();
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
    /// <returns>The Id of the seeded entry.</returns>
    private async Task<Guid> SeedPlaylistEntryAsync(Guid playlistId, Guid mediaItemId, long position)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid entryId = Guid.NewGuid();
        dbContext.PlaylistEntries.Add(_playlistEntryEntityFixture.Create(
            id: entryId,
            playlistId: playlistId,
            mediaItemId: mediaItemId,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: position));
        await dbContext.SaveChangesAsync();
        return entryId;
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
