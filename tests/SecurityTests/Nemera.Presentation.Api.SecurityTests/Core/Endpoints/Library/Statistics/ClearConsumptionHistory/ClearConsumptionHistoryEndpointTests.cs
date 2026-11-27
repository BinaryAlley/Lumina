#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// Contains security tests for the <see cref="ClearConsumptionHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/consumption-history";

    private static readonly DateTime s_startedOnUtc = new(2026, 5, 3, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 5, 3, 9, 10, 0, DateTimeKind.Utc);

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ClearConsumptionHistoryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task ClearConsumptionHistory_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync(PATH);

        // Assert
        await AssertUnauthorizedAsync(response, PATH);
    }

    [Fact]
    public async Task ClearConsumptionHistory_WhenCalled_ShouldDeleteOnlyTheCallersHistoryAndPreserveResumePoints()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid ownLibraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid ownRecordId = await SeedRecordAsync(userId, ownLibraryId);
        Guid ownOtherRecordId = await SeedRecordAsync(userId, ownLibraryId);
        Guid ownResumePointId = await SeedResumePointAsync(userId, ownLibraryId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserRecordId = await SeedRecordAsync(otherUserId, otherUserLibraryId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);

        await AssertRecordsAreDeletedAsync(ownRecordId, ownOtherRecordId);
        await AssertRecordsArePreservedAsync(otherUserRecordId);
        // Clearing the history is a separate user action from clearing the continue watching list, so resume points survive.
        await AssertResumePointsArePreservedAsync(ownResumePointId);
    }

    [Fact]
    public async Task ClearConsumptionHistory_WithSpoofedUserIdQueryParameter_ShouldIgnoreItAndClearOnlyTheCallersHistory()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid ownLibraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid ownRecordId = await SeedRecordAsync(userId, ownLibraryId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserRecordId = await SeedRecordAsync(otherUserId, otherUserLibraryId);
        string url = $"{PATH}?userId={otherUserId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        // The me route always targets the authenticated caller, so a spoofed query parameter cannot redirect the deletion.
        await AssertRecordsAreDeletedAsync(ownRecordId);
        await AssertRecordsArePreservedAsync(otherUserRecordId);
    }

    [Fact]
    public async Task ClearConsumptionHistory_WithInjectionInSpoofedUserIdQueryParameter_ShouldIgnoreItAndRemainSecure()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid ownLibraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid ownRecordId = await SeedRecordAsync(userId, ownLibraryId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserRecordId = await SeedRecordAsync(otherUserId, otherUserLibraryId);
        string maliciousUserId = "'; DROP TABLE MediaConsumptionRecords--";
        string url = $"{PATH}?userId={Uri.EscapeDataString(maliciousUserId)}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        Assert.DoesNotContain(maliciousUserId, content, StringComparison.Ordinal);

        // The unused query parameter never reaches the query builder, so the caller's own rows are deleted and the other user's rows survive.
        await AssertRecordsAreDeletedAsync(ownRecordId);
        await AssertRecordsArePreservedAsync(otherUserRecordId);
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
    /// Seeds a user distinct from the authenticated test user and returns its Id and username.
    /// </summary>
    /// <returns>The Id and username of the seeded user.</returns>
    private async Task<(Guid UserId, string Username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        UserEntity user = _userEntityFixture.Create(id: userId, username: username, password: "TestPass123!");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="title">The title of the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: title, libraryType: LibraryType.Music, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a consumption record of a music track of <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the record.</param>
    /// <param name="libraryId">The Id of the library that contains the consumed track.</param>
    /// <returns>The Id of the seeded consumption record.</returns>
    private async Task<Guid> SeedRecordAsync(Guid userId, Guid libraryId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid recordId = Guid.NewGuid();
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: Guid.NewGuid(),
            status: ConsumptionStatus.Stopped,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            endedOnUtc: s_lastActivityOnUtc.AddMinutes(1),
            consumedDurationMilliseconds: 90_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 45_000,
            lastPositionLocation: null,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0));
        await dbContext.SaveChangesAsync();
        return recordId;
    }

    /// <summary>
    /// Seeds a resume point of <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the resume point.</param>
    /// <param name="libraryId">The Id of the library that contains the media item.</param>
    /// <returns>The Id of the seeded resume point.</returns>
    private async Task<Guid> SeedResumePointAsync(Guid userId, Guid libraryId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid resumePointId = Guid.NewGuid();
        dbContext.MediaResumePoints.Add(_mediaResumePointEntityFixture.Create(
            id: resumePointId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: Guid.NewGuid(),
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 45_000,
            lastPositionLocation: null,
            lastProgressFraction: 0.5,
            lastConsumedOnUtc: s_lastActivityOnUtc));
        await dbContext.SaveChangesAsync();
        return resumePointId;
    }

    /// <summary>
    /// Asserts that the response is the unauthenticated problem document produced by the authentication middleware.
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
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
        AssertNoSensitiveData(content);
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
    /// Asserts that the resume points identified by <paramref name="resumePointIds"/> are still stored once the request transaction has completed.
    /// </summary>
    /// <param name="resumePointIds">The Ids of the resume points that must be preserved.</param>
    private async Task AssertResumePointsArePreservedAsync(params Guid[] resumePointIds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<MediaResumePointEntity> storedResumePoints = [];
        // The request transaction is committed or rolled back after the response has been sent, so a resume point that must survive is read until
        // that completion has run, instead of trusting the state observed right after the response.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            storedResumePoints = await dbContext.MediaResumePoints.AsNoTracking()
                .Where(resumePoint => resumePointIds.Contains(resumePoint.Id))
                .ToListAsync();
            Assert.Equal(resumePointIds.Length, storedResumePoints.Count);
            await Task.Delay(100);
        }
        Assert.Equal(resumePointIds.Length, storedResumePoints.Count);
    }

    /// <summary>
    /// Asserts that the per-user responses are not storable by any intermediary and vary by the authorization header.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertNonCacheable(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
    }

    /// <summary>
    /// Asserts that the response body carries no credentials, secrets, persistence details or stack traces.
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
    /// Removes the test users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
