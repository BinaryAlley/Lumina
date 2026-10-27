#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.CompleteConsumption;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.MediaConsumption.CompleteConsumption;

/// <summary>
/// Contains security tests for the <see cref="CompleteConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const long SEEDED_VERSION = 7;

    private static readonly DateTime s_startedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 3, 1, 10, 5, 0, DateTimeKind.Utc);

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly CompleteConsumptionRequestFixture _completeConsumptionRequestFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CompleteConsumptionEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task CompleteConsumption_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}/complete";
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(includePosition: false, progressFraction: 1, consumedDurationInSeconds: 10);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task CompleteConsumption_WhenSuccessful_ShouldReturnNonCacheableResponseWithoutSensitiveData()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (_, Guid recordId) = await SeedRecordAsync(userId, MediaKind.MusicTrack, ConsumptionStatus.InProgress, 0, 0.1, MediaPositionKind.TimeOffset, 5_000, null);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: nameof(MediaPositionKind.TimeOffset), timeOffsetSeconds: 60, includeLocation: false),
            progressFraction: 1,
            consumedDurationInSeconds: 120);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/complete";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);

        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(recordId, record!.Id);
        Assert.Equal(ConsumptionStatus.Completed, record.Status);
        Assert.NotNull(record.EndedOnUtc);
        Assert.Equal(120.0, record.ConsumedDurationInSeconds);
        Assert.Equal(1.0, record.LastProgressFraction);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, record.LastPosition!.Kind);
        Assert.Equal(60.0, record.LastPosition.TimeOffsetSeconds);
    }

    [Fact]
    public async Task CompleteConsumption_WhenRecordDoesNotExist_ShouldReturnNonCacheableNotFoundWithoutSensitiveData()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(includePosition: false, progressFraction: 1, consumedDurationInSeconds: 10);
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}/complete";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        string content = await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "General.NotFound",
            "ConsumptionRecordNotFound",
            url,
            "https://tools.ietf.org/html/rfc9110#section-15.5.5");
        AssertNonCacheable(response);
        AssertNoSensitiveData(content);
    }

    [Fact]
    public async Task CompleteConsumption_WhenAnotherUsersRecordIsTargeted_ShouldReturnNotFoundAndLeaveRecordUnchanged()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (_, Guid otherRecordId) = await SeedRecordAsync(otherUserId, MediaKind.MusicTrack, ConsumptionStatus.InProgress, 15_000, 0.2, MediaPositionKind.TimeOffset, 15_000, null);
        await CreateAuthenticatedUserAsync(_client);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: nameof(MediaPositionKind.TimeOffset), timeOffsetSeconds: 90, includeLocation: false),
            progressFraction: 1,
            consumedDurationInSeconds: 90);
        string url = $"/api/v1/users/me/consumption-records/{otherRecordId}/complete";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        string content = await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "General.NotFound",
            "ConsumptionRecordNotFound",
            url,
            "https://tools.ietf.org/html/rfc9110#section-15.5.5");
        AssertNoSensitiveData(content);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(record => record.Id == otherRecordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(ConsumptionStatus.InProgress, storedRecord!.Status);
        Assert.Null(storedRecord.EndedOnUtc);
        Assert.Equal(15_000, storedRecord.ConsumedDurationMilliseconds);
        Assert.Equal(0.2, storedRecord.LastProgressFraction);
        Assert.Equal(MediaPositionKind.TimeOffset, storedRecord.LastPositionKind);
        Assert.Equal(15_000, storedRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Equal(SEEDED_VERSION, storedRecord.Version);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task CompleteConsumption_WithSqlInjectionInRecordId_ShouldRejectWithoutLeaking(string maliciousRecordId)
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(includePosition: false, progressFraction: 1, consumedDurationInSeconds: 10);
        string url = $"/api/v1/users/me/consumption-records/{Uri.EscapeDataString(maliciousRecordId)}/complete";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        await AssertRejectedInjectionAsync(response, maliciousRecordId, url, "ConsumptionRecordIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task CompleteConsumption_WithUnknownPositionKind_ShouldRejectWithoutLeaking(string maliciousPositionKind)
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: maliciousPositionKind),
            progressFraction: 1,
            consumedDurationInSeconds: 10);
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}/complete";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        string content = await AssertValidationProblemAsync(response, url, "InvalidMediaPosition");
        AssertNoSensitiveData(content);
        Assert.DoesNotContain(maliciousPositionKind, content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task CompleteConsumption_WithOpaquePositionLocation_ShouldStoreItVerbatimWithoutLeaking(string location)
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (_, Guid recordId) = await SeedRecordAsync(userId, MediaKind.Book, ConsumptionStatus.InProgress, 0, 0.1, MediaPositionKind.Location, null, "chapter-1");
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: nameof(MediaPositionKind.Location), location: location, includeTimeOffsetSeconds: false),
            progressFraction: 1,
            consumedDurationInSeconds: 30);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/complete";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);

        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(ConsumptionStatus.Completed, record!.Status);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.Location, record.LastPosition!.Kind);
        Assert.Equal(location, record.LastPosition.Location);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(candidate => candidate.Id == recordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(location, storedRecord!.LastPositionLocation);
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
    /// Seeds a media library owned by <paramref name="userId"/> and a consumption record of a media item belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the record.</param>
    /// <param name="mediaKind">The kind of media that the record consumes.</param>
    /// <param name="status">The state of the seeded session.</param>
    /// <param name="consumedDurationMilliseconds">The consumed duration of the record, in milliseconds.</param>
    /// <param name="lastProgressFraction">The last reported progress fraction of the record.</param>
    /// <param name="lastPositionKind">The kind of the last known position.</param>
    /// <param name="lastPositionTimeOffsetMilliseconds">The last known position, in milliseconds, when it is a time offset.</param>
    /// <param name="lastPositionLocation">The last known opaque locator, when it is a location.</param>
    /// <returns>The Ids of the seeded library and consumption record.</returns>
    private async Task<(Guid LibraryId, Guid RecordId)> SeedRecordAsync(
        Guid userId,
        MediaKind mediaKind,
        ConsumptionStatus status,
        long consumedDurationMilliseconds,
        double lastProgressFraction,
        MediaPositionKind lastPositionKind,
        long? lastPositionTimeOffsetMilliseconds,
        string? lastPositionLocation)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        Guid recordId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId,
            userId: userId,
            title: "Media Library",
            libraryType: mediaKind == MediaKind.Book ? LibraryType.EBook : LibraryType.Music,
            contentLocations: [],
            shouldTrackConsumptionProgress: false));
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            status: status,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            consumedDurationMilliseconds: consumedDurationMilliseconds,
            lastPositionKind: lastPositionKind,
            lastPositionTimeOffsetMilliseconds: lastPositionTimeOffsetMilliseconds,
            lastPositionLocation: lastPositionLocation,
            lastProgressFraction: lastProgressFraction,
            consecutivePagesRead: 0,
            version: SEEDED_VERSION));
        await dbContext.SaveChangesAsync();
        return (libraryId, recordId);
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
    /// Asserts the shape of a problem details response with the provided status, title, detail, instance and type.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="statusCode">The expected HTTP status code.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedType">The expected problem type URI.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertProblemDetailsAsync(HttpResponseMessage response, HttpStatusCode statusCode, string expectedTitle, string expectedDetail, string expectedInstance, string expectedType)
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)statusCode, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedType, problemDetails["type"].GetString());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
        return content;
    }

    /// <summary>
    /// Asserts that the response is the standard validation problem of the complete consumption route.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorDescription">The error description expected under the validation errors.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorDescription)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorDescription, errors["General.Validation"]);
        return content;
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
            Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
            Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
            Assert.Equal("General.Validation", problemDetails["title"].GetString());
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
    /// Asserts that the serialized consumption record exposes only the fields of its documented response contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        Assert.DoesNotContain("version", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userId", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdOnUtc", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedOnUtc", content, StringComparison.OrdinalIgnoreCase);
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
