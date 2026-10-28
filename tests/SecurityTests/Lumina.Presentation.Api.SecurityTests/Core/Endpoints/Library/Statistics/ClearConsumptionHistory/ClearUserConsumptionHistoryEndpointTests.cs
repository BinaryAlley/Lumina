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
/// Contains security tests for the <see cref="ClearUserConsumptionHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearUserConsumptionHistoryEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH_PREFIX = "/api/v1/users/";
    private const string PATH_SUFFIX = "/consumption-history";

    private static readonly DateTime s_startedOnUtc = new(2026, 5, 4, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 5, 4, 9, 10, 0, DateTimeKind.Utc);

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
    private readonly List<Guid> _adminUserIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearUserConsumptionHistoryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ClearUserConsumptionHistoryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task ClearUserConsumptionHistory_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH_PREFIX}{Guid.NewGuid()}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task ClearUserConsumptionHistory_WhenCallerNamesThemselves_ShouldClearTheOwnHistory()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid libraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid recordId = await SeedRecordAsync(userId, libraryId);
        string url = $"{PATH_PREFIX}{userId}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);
        AssertNoSensitiveData(await response.Content.ReadAsStringAsync());
        await AssertRecordsAreDeletedAsync(recordId);
    }

    [Fact]
    public async Task ClearUserConsumptionHistory_WhenRegularUserNamesAnotherUser_ShouldReturnForbiddenAndPreserveTheHistory()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        (Guid targetUserId, _) = await SeedOtherUserAsync();
        Guid targetLibraryId = await SeedLibraryAsync(targetUserId, "Target User Library");
        Guid targetRecordId = await SeedRecordAsync(targetUserId, targetLibraryId);
        string url = $"{PATH_PREFIX}{targetUserId}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        string content = await AssertForbiddenAsync(response, url);
        AssertNonCacheable(response);
        AssertNoSensitiveData(content);
        await AssertRecordsArePreservedAsync(targetRecordId);
    }

    [Fact]
    public async Task ClearUserConsumptionHistory_WhenTargetUserDoesNotExist_ShouldReturnSameForbiddenResultAsForeignUser()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherUserLibraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid otherUserRecordId = await SeedRecordAsync(otherUserId, otherUserLibraryId);
        string foreignUrl = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";
        string missingUrl = $"{PATH_PREFIX}{Guid.NewGuid()}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage foreignResponse = await _client.DeleteAsync(foreignUrl);
        HttpResponseMessage missingResponse = await _client.DeleteAsync(missingUrl);

        // Assert
        await AssertForbiddenAsync(foreignResponse, foreignUrl);
        await AssertForbiddenAsync(missingResponse, missingUrl);
        Dictionary<string, JsonElement> foreignProblem = await ReadProblemDetailsAsync(foreignResponse);
        Dictionary<string, JsonElement> missingProblem = await ReadProblemDetailsAsync(missingResponse);
        Assert.Equal(missingProblem["status"].GetInt32(), foreignProblem["status"].GetInt32());
        Assert.Equal(missingProblem["title"].GetString(), foreignProblem["title"].GetString());
        Assert.Equal(missingProblem["detail"].GetString(), foreignProblem["detail"].GetString());
        Assert.Equal(missingProblem["type"].GetString(), foreignProblem["type"].GetString());
        await AssertRecordsArePreservedAsync(otherUserRecordId);
    }

    [Fact]
    public async Task ClearUserConsumptionHistory_WhenAdminNamesAnotherUser_ShouldClearThatUsersHistory()
    {
        // Arrange
        (Guid targetUserId, _) = await SeedOtherUserAsync();
        Guid targetLibraryId = await SeedLibraryAsync(targetUserId, "Target User Library");
        Guid targetRecordId = await SeedRecordAsync(targetUserId, targetLibraryId);
        Guid targetResumePointId = await SeedResumePointAsync(targetUserId, targetLibraryId);
        string url = $"{PATH_PREFIX}{targetUserId}{PATH_SUFFIX}";
        HttpClient adminClient = _apiFactory.CreateClient();
        Guid adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(adminClient);
        _adminUserIds.Add(adminUserId);

        try
        {
            // Act
            HttpResponseMessage response = await adminClient.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertNonCacheable(response);
            AssertNoSensitiveData(await response.Content.ReadAsStringAsync());
            await AssertRecordsAreDeletedAsync(targetRecordId);
            await AssertResumePointsArePreservedAsync(targetResumePointId);
        }
        finally
        {
            adminClient.Dispose();
        }
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task ClearUserConsumptionHistory_WithInjectionInUserIdRouteValue_ShouldRejectWithoutDeletingRows(string maliciousUserId)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid libraryId = await SeedLibraryAsync(userId, "Owner Library");
        Guid recordId = await SeedRecordAsync(userId, libraryId);
        string url = $"{PATH_PREFIX}{Uri.EscapeDataString(maliciousUserId)}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertRejectedInjectionAsync(response, maliciousUserId, url, "TargetUserIdIsInvalid");
        AssertNonCacheable(response);
        await AssertRecordsArePreservedAsync(recordId);
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
    /// Asserts that the response is the exact not authorized problem document and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertForbiddenAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        return content;
    }

    /// <summary>
    /// Reads the problem details document of <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The parsed problem details fields.</returns>
    private async Task<Dictionary<string, JsonElement>> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        return problemDetails!;
    }

    /// <summary>
    /// Asserts that a malicious route value is rejected without disclosing internals or reaching the data access layer.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="payload">The malicious value that was sent.</param>
    /// <param name="requestPath">The request path the value was sent in.</param>
    /// <param name="expectedErrorCode">The validation error code expected when the value reaches the validator.</param>
    private async Task AssertRejectedInjectionAsync(HttpResponseMessage response, string payload, string requestPath, string expectedErrorCode)
    {
        string content = await response.Content.ReadAsStringAsync();
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
            Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
            Assert.Equal("General.Validation", problemDetails["title"].GetString());
            Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
            Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

            // The instance is the request path with the rejected payload still in place, independent of how the server escapes it.
            string escapedPayload = Uri.EscapeDataString(payload);
            int payloadIndex = requestPath.IndexOf(escapedPayload, StringComparison.Ordinal);
            string? instance = problemDetails["instance"].GetString();
            Assert.NotNull(instance);
            Assert.True(payloadIndex >= 0, "The escaped payload must appear in the request path.");
            Assert.StartsWith(requestPath[..payloadIndex], instance, StringComparison.Ordinal);
            Assert.EndsWith(requestPath[(payloadIndex + escapedPayload.Length)..], instance, StringComparison.Ordinal);

            Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
            Assert.NotNull(errors);
            Assert.Contains("General.Validation", errors!.Keys);
            Assert.Contains(expectedErrorCode, errors["General.Validation"]);
        }
        else
        {
            // A payload that carries encoded path separators is rejected by routing before it reaches the validator. That is still a safe rejection, so only it may take this branch.
            Assert.Contains("/", payload, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        AssertNoSensitiveData(content);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
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
    /// Removes the test users and admin users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
        foreach (Guid userId in _adminUserIds)
            _apiFactory.RemoveAdminUserAsync(userId).GetAwaiter().GetResult();
    }
}
