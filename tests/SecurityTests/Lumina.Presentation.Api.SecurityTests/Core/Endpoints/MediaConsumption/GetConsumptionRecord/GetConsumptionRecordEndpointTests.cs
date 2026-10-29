#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetConsumptionRecord;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.MediaConsumption.GetConsumptionRecord;

/// <summary>
/// Contains security tests for the <see cref="GetConsumptionRecordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";

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
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetConsumptionRecordEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenSuccessful_ShouldReturnNonCacheableResponseWithoutSensitiveData()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid recordId = await SeedRecordAsync(userId, ConsumptionStatus.InProgress, 15_000, 0.2, 15_000);
        string url = $"/api/v1/users/me/consumption-records/{recordId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);

        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(recordId, record!.Id);
        Assert.Equal(MediaKind.MusicTrack, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(15.0, record.ConsumedDurationInSeconds);
        Assert.Equal(0.2, record.LastProgressFraction);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, record.LastPosition!.Kind);
        Assert.Equal(15.0, record.LastPosition.TimeOffsetSeconds);
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenRecordDoesNotExist_ShouldReturnNonCacheableNotFoundWithoutSensitiveData()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

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
    public async Task GetConsumptionRecord_WhenAnotherUsersRecordIsRequested_ShouldReturnNotFoundWithoutDisclosingExistence()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherRecordId = await SeedRecordAsync(otherUserId, ConsumptionStatus.InProgress, 42_000, 0.4, 42_000);
        await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/users/me/consumption-records/{otherRecordId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Dictionary<string, JsonElement> foreignProblem = await ReadProblemDetailsAsync(response);
        HttpResponseMessage missingResponse = await _client.GetAsync($"/api/v1/users/me/consumption-records/{Guid.NewGuid()}");
        Dictionary<string, JsonElement> missingProblem = await ReadProblemDetailsAsync(missingResponse);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        Assert.Equal(missingProblem["status"].GetInt32(), foreignProblem["status"].GetInt32());
        Assert.Equal(missingProblem["title"].GetString(), foreignProblem["title"].GetString());
        Assert.Equal(missingProblem["detail"].GetString(), foreignProblem["detail"].GetString());
        Assert.Equal(missingProblem["type"].GetString(), foreignProblem["type"].GetString());

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(record => record.Id == otherRecordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(otherUserId, storedRecord!.UserId);
        Assert.Equal(ConsumptionStatus.InProgress, storedRecord.Status);
        Assert.Equal(42_000, storedRecord.ConsumedDurationMilliseconds);
        Assert.Equal(0.4, storedRecord.LastProgressFraction);
    }

    [Fact]
    public async Task GetConsumptionRecord_WithSpoofedUserIdQueryParameter_ShouldIgnoreItAndReturnOwnRecord()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid ownRecordId = await SeedRecordAsync(userId, ConsumptionStatus.InProgress, 10_000, 0.1, 10_000);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherRecordId = await SeedRecordAsync(otherUserId, ConsumptionStatus.Completed, 90_000, 1.0, 90_000);
        string url = $"/api/v1/users/me/consumption-records/{ownRecordId}?userId={otherUserId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(ownRecordId, record!.Id);
        Assert.NotEqual(otherRecordId, record.Id);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(candidate => candidate.Id == otherRecordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(otherUserId, storedRecord!.UserId);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetConsumptionRecord_WithSqlInjectionInRecordId_ShouldRejectWithoutLeaking(string maliciousRecordId)
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/users/me/consumption-records/{Uri.EscapeDataString(maliciousRecordId)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertRejectedInjectionAsync(response, maliciousRecordId, url, "ConsumptionRecordIdCannotBeEmpty");
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
    /// Seeds a music library owned by <paramref name="userId"/> and a consumption record of a music track belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the record.</param>
    /// <param name="status">The state of the seeded session.</param>
    /// <param name="consumedDurationMilliseconds">The consumed duration of the record, in milliseconds.</param>
    /// <param name="lastProgressFraction">The last reported progress fraction of the record.</param>
    /// <param name="lastPositionTimeOffsetMilliseconds">The last known position, in milliseconds.</param>
    /// <returns>The Id of the seeded consumption record.</returns>
    private async Task<Guid> SeedRecordAsync(
        Guid userId,
        ConsumptionStatus status,
        long consumedDurationMilliseconds,
        double lastProgressFraction,
        long lastPositionTimeOffsetMilliseconds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        Guid recordId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Music Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            status: status,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            consumedDurationMilliseconds: consumedDurationMilliseconds,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: lastPositionTimeOffsetMilliseconds,
            lastProgressFraction: lastProgressFraction,
            consecutivePagesRead: 0));
        await dbContext.SaveChangesAsync();
        return recordId;
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
