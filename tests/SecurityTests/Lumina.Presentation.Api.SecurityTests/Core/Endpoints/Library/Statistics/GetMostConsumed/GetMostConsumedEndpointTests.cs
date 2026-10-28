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
/// Contains security tests for the <see cref="GetMostConsumedEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/consumption-history/most-consumed";

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

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetMostConsumedEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetMostConsumed_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = PATH;

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetMostConsumed_WhenSuccessful_ShouldReturnOnlyCallersDataWithoutSensitiveData()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid callerLibraryId, Guid callerMediaItemId) = await SeedConsumedTrackAsync(callerId, "Caller-Most-Consumed");
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (_, Guid otherMediaItemId) = await SeedConsumedTrackAsync(otherUserId, "Foreign-Most-Consumed");
        string url = PATH;

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        Assert.DoesNotContain(otherMediaItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foreign-Most-Consumed", content, StringComparison.Ordinal);

        PaginatedResponse<ConsumedItemResponse>? history = JsonSerializer.Deserialize<PaginatedResponse<ConsumedItemResponse>>(content, _jsonOptions);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(callerMediaItemId, item.MediaItemId);
        Assert.Equal(callerLibraryId, item.LibraryId);
        Assert.Equal("Caller-Most-Consumed", item.Title);
    }

    [Fact]
    public async Task GetMostConsumed_WithSpoofedUserIdQueryParameter_ShouldIgnoreItAndReturnCallersData()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        (_, Guid callerMediaItemId) = await SeedConsumedTrackAsync(callerId, "Caller-Spoof-Target");
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (_, Guid otherMediaItemId) = await SeedConsumedTrackAsync(otherUserId, "Foreign-Spoof-Target");
        string url = $"{PATH}?userId={otherUserId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        PaginatedResponse<ConsumedItemResponse>? history = JsonSerializer.Deserialize<PaginatedResponse<ConsumedItemResponse>>(content, _jsonOptions);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(callerMediaItemId, item.MediaItemId);
        Assert.NotEqual(otherMediaItemId, item.MediaItemId);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetMostConsumed_WithInjectionInLibraryId_ShouldRejectWithoutLeakingData(string maliciousLibraryId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedConsumedTrackAsync(callerId, "Caller-Injection-Target");
        string url = $"{PATH}?libraryId={Uri.EscapeDataString(maliciousLibraryId)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, PATH, "LibraryIdCannotBeEmpty");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(2, await dbContext.MediaConsumptionRecords.CountAsync(record => record.UserId == callerId));
    }

    [Theory]
    [InlineData("999")] // out of range numeric value
    [InlineData("not-a-media-kind")] // textual value that is not an enum member
    public async Task GetMostConsumed_WithUnsupportedMediaKind_ShouldNotReturnDataOrLeakInternals(string mediaKind)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedConsumedTrackAsync(callerId, "Caller-Media-Kind-Target");
        string url = $"{PATH}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        // The media kind is carried as text, so an unknown or numeric name reaches the validator and is reported as an
        // unsupported kind instead of being rejected by request binding.
        await AssertRejectedMediaKindAsync(response, PATH);
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
    /// Asserts the shape of a validation problem details response and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorCode)
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
    /// Removes the test users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
