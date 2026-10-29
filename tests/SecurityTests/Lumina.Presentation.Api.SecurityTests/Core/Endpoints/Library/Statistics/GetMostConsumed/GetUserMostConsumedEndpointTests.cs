#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetMostConsumed;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Statistics.GetMostConsumed;

/// <summary>
/// Contains security tests for the <see cref="GetUserMostConsumedEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserMostConsumedEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH_PREFIX = "/api/v1/users/";
    private const string PATH_SUFFIX = "/consumption-history/most-consumed";

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
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];
    private readonly List<Guid> _adminUserIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserMostConsumedEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetUserMostConsumedEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetUserMostConsumed_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH_PREFIX}{Guid.NewGuid()}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetUserMostConsumed_WhenAnotherUsersHistoryIsRequested_ShouldReturnForbiddenWithoutDisclosingData()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (_, Guid otherMediaItemId) = await SeedConsumedTrackAsync(otherUserId, "Foreign-Most-Consumed");
        string url = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertForbiddenAsync(response, url);
        AssertNonCacheable(response);
        Assert.DoesNotContain(otherMediaItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foreign-Most-Consumed", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetUserMostConsumed_WhenTargetUserDoesNotExist_ShouldReturnSameForbiddenResultAsForeignUser()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        await SeedConsumedTrackAsync(otherUserId, "Foreign-Indistinguishable");
        string foreignUrl = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";
        string missingUrl = $"{PATH_PREFIX}{Guid.NewGuid()}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage foreignResponse = await _client.GetAsync(foreignUrl);
        HttpResponseMessage missingResponse = await _client.GetAsync(missingUrl);

        // Assert
        await AssertForbiddenAsync(foreignResponse, foreignUrl);
        await AssertForbiddenAsync(missingResponse, missingUrl);
        Dictionary<string, JsonElement> foreignProblem = await ReadProblemDetailsAsync(foreignResponse);
        Dictionary<string, JsonElement> missingProblem = await ReadProblemDetailsAsync(missingResponse);
        Assert.Equal(missingProblem["status"].GetInt32(), foreignProblem["status"].GetInt32());
        Assert.Equal(missingProblem["title"].GetString(), foreignProblem["title"].GetString());
        Assert.Equal(missingProblem["detail"].GetString(), foreignProblem["detail"].GetString());
        Assert.Equal(missingProblem["type"].GetString(), foreignProblem["type"].GetString());
    }

    [Fact]
    public async Task GetUserMostConsumed_WhenAdminRequestsAnotherUser_ShouldReturnOnlyThatUsersData()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (_, Guid otherMediaItemId) = await SeedConsumedTrackAsync(otherUserId, "Foreign-Most-Consumed");
        HttpClient adminClient = _apiFactory.CreateClient();
        Guid adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(adminClient);
        _adminUserIds.Add(adminUserId);
        string url = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";

        try
        {
            // Act
            HttpResponseMessage response = await adminClient.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertNonCacheable(response);

            string content = await response.Content.ReadAsStringAsync();
            AssertNoSensitiveData(content);
            AssertNoDatabaseOnlyFields(content);

            PaginatedResponse<ConsumedItemResponse>? history = JsonSerializer.Deserialize<PaginatedResponse<ConsumedItemResponse>>(content, _jsonOptions);
            Assert.NotNull(history);
            ConsumedItemResponse item = Assert.Single(history!.Data);
            Assert.Equal(otherMediaItemId, item.MediaItemId);
            Assert.Equal("Foreign-Most-Consumed", item.Title);
        }
        finally
        {
            adminClient.Dispose();
        }
    }

    [Fact]
    public async Task GetUserMostConsumed_WhenCallerNamesThemselves_ShouldReturnOwnData()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        (_, Guid callerMediaItemId) = await SeedConsumedTrackAsync(callerId, "Caller-Most-Consumed");
        string url = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        PaginatedResponse<ConsumedItemResponse>? history = JsonSerializer.Deserialize<PaginatedResponse<ConsumedItemResponse>>(content, _jsonOptions);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(callerMediaItemId, item.MediaItemId);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetUserMostConsumed_WithInjectionInUserIdRouteValue_ShouldRejectWithoutLeakingData(string maliciousUserId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedConsumedTrackAsync(callerId, "Caller-Injection-Target");
        string url = $"{PATH_PREFIX}{Uri.EscapeDataString(maliciousUserId)}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertRejectedRouteValueAsync(response, maliciousUserId, PATH_PREFIX, PATH_SUFFIX, "TargetUserIdIsInvalid");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousUserId, content, StringComparison.Ordinal);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(2, await dbContext.MediaConsumptionRecords.CountAsync(record => record.UserId == callerId));
    }

    [Theory]
    [InlineData("999")] // out of range numeric value
    [InlineData("not-a-media-kind")] // textual value that is not an enum member
    public async Task GetUserMostConsumed_WithUnsupportedMediaKind_ShouldNotReturnDataOrLeakInternals(string mediaKind)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedConsumedTrackAsync(callerId, "Caller-Media-Kind-Target");
        string path = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        // The media kind is carried as text, so an unknown or numeric name reaches the validator and is reported as an
        // unsupported kind instead of being rejected by request binding.
        await AssertRejectedMediaKindAsync(response, path);
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
    /// Seeds a music library owned by <paramref name="userId"/> and two completed consumption sessions of one of its tracks.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the history.</param>
    /// <param name="title">The title of the consumed track.</param>
    /// <returns>The Ids of the seeded library and of the consumed media item.</returns>
    private async Task<(Guid LibraryId, Guid MediaItemId)> SeedConsumedTrackAsync(Guid userId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        string suffix = Guid.NewGuid().ToString("N");
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: $"Library-{suffix}", libraryType: LibraryType.Music, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: $"Artist-{suffix}", includeAlbums: false, includeContributors: false, includeMetadata: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: $"Album-{suffix}", includeTracks: false, includeMetadata: false));
        dbContext.Tracks.Add(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: $"/music/{trackId:N}.flac", title: title, includeMetadata: false));
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: Guid.NewGuid(),
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            lastProgressFraction: 1.0,
            consecutivePagesRead: 0));
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: Guid.NewGuid(),
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: s_startedOnUtc.AddDays(1),
            lastActivityOnUtc: s_lastActivityOnUtc.AddDays(1),
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            lastProgressFraction: 1.0,
            consecutivePagesRead: 0));
        await dbContext.SaveChangesAsync();
        return (libraryId, trackId);
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
        AssertNoSensitiveData(content);
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
    /// Asserts that a malicious route value is rejected without disclosing internals.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="payload">The malicious value that was sent.</param>
    /// <param name="pathPrefix">The request path that precedes the malicious value.</param>
    /// <param name="pathSuffix">The request path that follows the malicious value.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertRejectedRouteValueAsync(HttpResponseMessage response, string payload, string pathPrefix, string pathSuffix, string expectedErrorCode)
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
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        // The instance is the request path with the rejected payload in place, so decoding the middle segment must give the payload back.
        string? instance = problemDetails["instance"].GetString();
        Assert.NotNull(instance);
        Assert.StartsWith(pathPrefix, instance, StringComparison.Ordinal);
        Assert.EndsWith(pathSuffix, instance, StringComparison.Ordinal);
        string encodedSegment = instance![pathPrefix.Length..(instance.Length - pathSuffix.Length)];
        Assert.Equal(payload, Uri.UnescapeDataString(encodedSegment));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorCode, errors["General.Validation"]);
        AssertNoSensitiveData(content);
        return content;
    }

    /// <summary>
    /// Asserts that an unparseable media kind is rejected without returning data or disclosing internals.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertRejectedMediaKindAsync(HttpResponseMessage response, string expectedInstance)
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
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains("UnsupportedMediaKind", errors["General.Validation"]);

        AssertNonCacheable(response);
        AssertNoSensitiveData(content);
        Assert.DoesNotContain("\"data\"", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the response is not storable by any intermediary and varies by the authorization header.
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
    /// Asserts that the serialized consumed items expose only the fields of their documented response contract.
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
