#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.ClearResumePoints;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Statistics.ClearResumePoints;

/// <summary>
/// Contains security tests for the <see cref="ClearResumePointsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/media-resume-points";

    private static readonly DateTime s_startedOnUtc = new(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 5, 5, 9, 10, 0, DateTimeKind.Utc);

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
    /// Initializes a new instance of the <see cref="ClearResumePointsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ClearResumePointsEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task ClearResumePoints_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync(PATH);

        // Assert
        await AssertUnauthorizedAsync(response, PATH);
    }

    [Fact]
    public async Task ClearResumePoints_WhenLibraryIdIsOmitted_ShouldDeleteOnlyTheCallersResumePointsAndPreserveHistory()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid firstLibraryId = await SeedLibraryAsync(userId, "First Owner Library");
        Guid secondLibraryId = await SeedLibraryAsync(userId, "Second Owner Library");
        Guid firstResumePointId = await SeedResumePointAsync(userId, firstLibraryId);
        Guid secondResumePointId = await SeedResumePointAsync(userId, secondLibraryId);
        Guid recordId = await SeedRecordAsync(userId, firstLibraryId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserResumePointId = await SeedResumePointAsync(otherUserId, otherUserLibraryId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);

        await AssertResumePointsAreDeletedAsync(firstResumePointId, secondResumePointId);
        await AssertResumePointsArePreservedAsync(otherUserResumePointId);
        // Removing the resume points does not touch the history, so the consumption records survive.
        await AssertRecordsArePreservedAsync(recordId);
    }

    [Fact]
    public async Task ClearResumePoints_WithLibraryId_ShouldDeleteOnlyTheCallersResumePointsInThatLibrary()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid targetLibraryId = await SeedLibraryAsync(userId, "Target Owner Library");
        Guid otherLibraryId = await SeedLibraryAsync(userId, "Other Owner Library");
        Guid targetResumePointId = await SeedResumePointAsync(userId, targetLibraryId);
        Guid otherResumePointId = await SeedResumePointAsync(userId, otherLibraryId);
        Guid recordId = await SeedRecordAsync(userId, targetLibraryId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserResumePointId = await SeedResumePointAsync(otherUserId, otherUserLibraryId);
        string url = $"{PATH}?libraryId={targetLibraryId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);
        AssertNoSensitiveData(await response.Content.ReadAsStringAsync());

        await AssertResumePointsAreDeletedAsync(targetResumePointId);
        await AssertResumePointsArePreservedAsync(otherResumePointId, otherUserResumePointId);
        await AssertRecordsArePreservedAsync(recordId);
    }

    [Fact]
    public async Task ClearResumePoints_WithSpoofedUserIdQueryParameter_ShouldIgnoreItAndNeverDeleteTheOtherUsersResumePoints()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid ownLibraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid ownResumePointId = await SeedResumePointAsync(userId, ownLibraryId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserResumePointId = await SeedResumePointAsync(otherUserId, otherUserLibraryId);
        string url = $"{PATH}?libraryId={otherUserLibraryId}&userId={otherUserId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        // The me route always targets the authenticated caller, so the spoofed user Id cannot turn the request into a deletion of the other user's rows.
        await AssertResumePointsArePreservedAsync(otherUserResumePointId, ownResumePointId);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaResumePoints--")] // destructive injection
    [InlineData("1; DELETE FROM MediaResumePoints")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task ClearResumePoints_WithInjectionInLibraryIdQueryParameter_ShouldRejectWithoutDeletingRows(string maliciousLibraryId)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid libraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid resumePointId = await SeedResumePointAsync(userId, libraryId);
        string url = $"{PATH}?libraryId={Uri.EscapeDataString(maliciousLibraryId)}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertRejectedQueryInjectionAsync(response, maliciousLibraryId, PATH, "LibraryIdCannotBeEmpty");
        AssertNonCacheable(response);
        await AssertResumePointsArePreservedAsync(resumePointId);
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
    /// Asserts that a malicious query value is rejected with the exact RFC 4918 validation problem shape.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="payload">The malicious value that was sent.</param>
    /// <param name="requestPath">The request path the value was sent on.</param>
    /// <param name="expectedErrorCode">The validation error code expected when the value reaches the validator.</param>
    private async Task AssertRejectedQueryInjectionAsync(HttpResponseMessage response, string payload, string requestPath, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal(requestPath, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorCode, errors["General.Validation"]);

        AssertNoSensitiveData(content);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the resume points identified by <paramref name="resumePointIds"/> are no longer stored once the request transaction has completed.
    /// </summary>
    /// <param name="resumePointIds">The Ids of the resume points that must be deleted.</param>
    private async Task AssertResumePointsAreDeletedAsync(params Guid[] resumePointIds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<MediaResumePointEntity> storedResumePoints = [];
        // The request transaction is committed or rolled back after the response has been sent, so the stored state is read until that completion
        // has run, instead of assuming that it already has.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            storedResumePoints = await dbContext.MediaResumePoints.AsNoTracking()
                .Where(resumePoint => resumePointIds.Contains(resumePoint.Id))
                .ToListAsync();
            if (storedResumePoints.Count == 0)
                return;
            await Task.Delay(100);
        }
        Assert.Empty(storedResumePoints);
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
