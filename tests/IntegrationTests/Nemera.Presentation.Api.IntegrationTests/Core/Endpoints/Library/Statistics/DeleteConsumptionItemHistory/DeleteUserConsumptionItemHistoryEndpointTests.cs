#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Contains integration tests for the <see cref="DeleteUserConsumptionItemHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteUserConsumptionItemHistoryEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_startedOnUtc = new(2026, 4, 3, 18, 5, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 4, 3, 18, 40, 0, DateTimeKind.Utc);

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserConsumptionItemHistoryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteUserConsumptionItemHistoryEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        _seededUsernames.Add(_apiFactory.TestUsername!);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenCurrentUserNamesThemselves_ShouldDeleteTheItemRecords()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Current User Library");
        Guid mediaItemId = Guid.NewGuid();
        Guid firstRecordId = await SeedConsumptionRecordAsync(userId, libraryId, MediaKind.MusicTrack, mediaItemId, 60_000);
        Guid secondRecordId = await SeedConsumptionRecordAsync(userId, libraryId, MediaKind.MusicTrack, mediaItemId, 90_000);
        string url = $"/api/v1/users/{userId}/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{mediaItemId}/{libraryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        await AssertRecordsAreDeletedAsync(firstRecordId, secondRecordId);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenRegularUserNamesAnotherUser_ShouldReturnForbiddenAndPreserveTheRecords()
    {
        // Arrange
        _ = GetCurrentUserId();
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, LibraryType.Music, "Other User Library");
        Guid mediaItemId = Guid.NewGuid();
        Guid otherUserRecordId = await SeedConsumptionRecordAsync(otherUserId, otherUserLibraryId, MediaKind.MusicTrack, mediaItemId, 60_000);
        string url = $"/api/v1/users/{otherUserId}/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{mediaItemId}/{otherUserLibraryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertForbiddenAsync(response, url);
        AssertCacheHeaders(response);
        await AssertRecordsArePreservedAsync(otherUserRecordId);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenAdminNamesAnotherUser_ShouldDeleteThatUsersItemRecords()
    {
        // Arrange
        _ = GetCurrentUserId();
        Guid targetUserId = await SeedOtherUserAsync();
        Guid targetLibraryId = await SeedLibraryAsync(targetUserId, LibraryType.Music, "Target User Library");
        Guid mediaItemId = Guid.NewGuid();
        Guid targetRecordId = await SeedConsumptionRecordAsync(targetUserId, targetLibraryId, MediaKind.MusicTrack, mediaItemId, 60_000);
        string url = $"/api/v1/users/{targetUserId}/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{mediaItemId}/{targetLibraryId}";
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();
        _seededUsernames.Add(_apiFactory.TestUsername!);

        try
        {
            // Act
            HttpResponseMessage response = await adminClient.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertCacheHeaders(response);
            await AssertRecordsAreDeletedAsync(targetRecordId);
        }
        finally
        {
            adminClient.Dispose();
        }
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenTargetUserDoesNotOwnTheItem_ShouldSucceedAndPreserveThatUsersRecords()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Current User Library");
        Guid ownRecordId = await SeedConsumptionRecordAsync(userId, libraryId, MediaKind.MusicTrack, Guid.NewGuid(), 60_000);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, LibraryType.Music, "Other User Library");
        Guid mediaItemId = Guid.NewGuid();
        Guid otherUserRecordId = await SeedConsumptionRecordAsync(otherUserId, otherUserLibraryId, MediaKind.MusicTrack, mediaItemId, 60_000);
        string url = $"/api/v1/users/{userId}/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{mediaItemId}/{otherUserLibraryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        await AssertRecordsArePreservedAsync(ownRecordId, otherUserRecordId);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenCalledTwice_ShouldSucceedAndStayDeleted()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Current User Library");
        Guid mediaItemId = Guid.NewGuid();
        Guid recordId = await SeedConsumptionRecordAsync(userId, libraryId, MediaKind.MusicTrack, mediaItemId, 60_000);
        string url = $"/api/v1/users/{userId}/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{mediaItemId}/{libraryId}";

        // Act
        HttpResponseMessage firstResponse = await _client.DeleteAsync(url);
        await AssertRecordsAreDeletedAsync(recordId);
        HttpResponseMessage secondResponse = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        AssertCacheHeaders(secondResponse);
        await AssertRecordsAreDeletedAsync(recordId);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenUserIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = $"/api/v1/users/not-a-guid/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{Guid.NewGuid()}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(path);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "TargetUserIdIsInvalid");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string path = $"/api/v1/users/{userId}/consumption-history/media-items/{nameof(MediaKind.None)}/{Guid.NewGuid()}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(path);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task DeleteUserConsumptionItemHistory_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/users/{Guid.NewGuid()}/consumption-history/media-items/{nameof(MediaKind.MusicTrack)}/{Guid.NewGuid()}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    /// <summary>
    /// Seeds a media library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="libraryType">The type of the library.</param>
    /// <param name="title">The title of the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId, LibraryType libraryType, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: title, libraryType: libraryType, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a consumption record of a media item for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the record.</param>
    /// <param name="libraryId">The Id of the library that contains the media item.</param>
    /// <param name="mediaKind">The kind of the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="consumedDurationMilliseconds">The active duration consumed by the session, in milliseconds.</param>
    /// <returns>The Id of the seeded record.</returns>
    private async Task<Guid> SeedConsumptionRecordAsync(
        Guid userId,
        Guid libraryId,
        MediaKind mediaKind,
        Guid mediaItemId,
        long consumedDurationMilliseconds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            endedOnUtc: s_lastActivityOnUtc.AddMinutes(1),
            consumedDurationMilliseconds: consumedDurationMilliseconds,
            lastPositionKind: mediaKind == MediaKind.Book ? MediaPositionKind.Location : MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: mediaKind == MediaKind.Book ? null : 45_000,
            lastPositionLocation: mediaKind == MediaKind.Book ? "chapter-1" : null,
            lastProgressFraction: 0.5,
            consecutivePagesRead: mediaKind == MediaKind.Book ? 10 : 0);
        dbContext.MediaConsumptionRecords.Add(record);
        await dbContext.SaveChangesAsync();
        return recordId;
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
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
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Asserts that the consumption records identified by <paramref name="recordIds"/> are no longer stored once the request transaction has completed.
    /// </summary>
    /// <param name="recordIds">The Ids of the consumption records that must be deleted.</param>
    private async Task AssertRecordsAreDeletedAsync(params Guid[] recordIds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<MediaConsumptionRecordEntity> storedRecords = [];
        // The request transaction is committed or rolled back after the response has been sent, so the stored state is read until that completion
        // has run, instead of assuming that it already has.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            storedRecords = await dbContext.MediaConsumptionRecords.AsNoTracking()
                .Where(record => recordIds.Contains(record.Id))
                .ToListAsync();
            if (storedRecords.Count == 0)
                return;
            await Task.Delay(100);
        }
        Assert.Empty(storedRecords);
    }

    /// <summary>
    /// Asserts that the consumption records identified by <paramref name="recordIds"/> are still stored once the request transaction has completed.
    /// </summary>
    /// <param name="recordIds">The Ids of the consumption records that must be preserved.</param>
    private async Task AssertRecordsArePreservedAsync(params Guid[] recordIds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<MediaConsumptionRecordEntity> storedRecords = [];
        // The request transaction is committed or rolled back after the response has been sent, so a record that must survive is read until that
        // completion has run, instead of trusting the state observed right after the response.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            storedRecords = await dbContext.MediaConsumptionRecords.AsNoTracking()
                .Where(record => recordIds.Contains(record.Id))
                .ToListAsync();
            Assert.Equal(recordIds.Length, storedRecords.Count);
            await Task.Delay(100);
        }
        Assert.Equal(recordIds.Length, storedRecords.Count);
    }

    /// <summary>
    /// Asserts that the response carries the cache headers of a private, authorization varying resource.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertCacheHeaders(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Cache-Control", out IEnumerable<string>? cacheControlValues));
        string cacheControl = string.Join(", ", cacheControlValues!);
        Assert.Contains("private", cacheControl);
        Assert.Contains("no-store", cacheControl);
        Assert.True(response.Headers.TryGetValues("Vary", out IEnumerable<string>? varyValues));
        Assert.Contains("Authorization", varyValues!);
    }

    /// <summary>
    /// Asserts that the response is an unprocessable entity problem details carrying the expected validation error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCodes">The validation error codes that must all be present.</param>
    private async Task AssertUnprocessableEntityAsync(HttpResponseMessage response, string expectedInstance, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        string[] expectedValidationErrors = [.. expectedErrorCodes.Order()];
        string[] actualValidationErrors = [.. errors["General.Validation"].Order()];
        Assert.Equal(expectedValidationErrors, actualValidationErrors);
    }

    /// <summary>
    /// Asserts that the response is a forbidden problem details carrying the exact not authorized shape.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertForbiddenAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    /// <summary>
    /// Asserts that the response is an unauthorized problem details carrying the expected request instance path.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertUnauthorizedAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
    }

    /// <summary>
    /// Removes every user created by the test, together with the libraries it owns.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        foreach (string username in _seededUsernames)
        {
            UserEntity? user = dbContext.Users
                .Include(candidate => candidate.Libraries)
                .FirstOrDefault(candidate => candidate.Username == username);
            if (user is null)
                continue;
            dbContext.Libraries.RemoveRange(user.Libraries);
            dbContext.Users.Remove(user);
        }
        await dbContext.SaveChangesAsync();
    }
}
