#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains integration tests for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_musicStartedOnUtc = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookStartedOnUtc = new(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc);

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetAllUsersConsumptionStatisticsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated admin API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedAdminClientAsync();
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByAdmin_ShouldReturnPerUserSummariesAndCombinedTotals()
    {
        // Arrange
        (Guid firstUserId, Guid firstUserLibraryId) = await SeedMusicOnlyUserAsync();
        (Guid secondUserId, Guid secondUserLibraryId) = await SeedBookOnlyUserAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/consumption-statistics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        AllUsersConsumptionStatisticsResponse? statistics = await DeserializeAsync<AllUsersConsumptionStatisticsResponse>(response);
        Assert.NotNull(statistics);

        ConsumptionSummaryResponse firstUser = Assert.Single(statistics!.Users, summary => summary.UserId == firstUserId);
        Assert.Equal(100d, firstUser.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, firstUser.CountedSessionCount);
        Assert.Equal(1, firstUser.CompletedSessionCount);
        Assert.Equal(1, firstUser.StoppedSessionCount);
        Assert.Equal(0.5, firstUser.CompletionRate, 6);
        MediaKindConsumptionSummaryResponse firstUserMusic = Assert.Single(firstUser.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, firstUserMusic.MediaKind);
        Assert.Equal(100d, firstUserMusic.TotalConsumedDurationInSeconds, 3);

        ConsumptionSummaryResponse secondUser = Assert.Single(statistics.Users, summary => summary.UserId == secondUserId);
        Assert.Equal(400d, secondUser.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(1, secondUser.CountedSessionCount);
        Assert.Equal(1, secondUser.CompletedSessionCount);
        Assert.Equal(0, secondUser.StoppedSessionCount);
        Assert.Equal(1d, secondUser.CompletionRate, 6);
        MediaKindConsumptionSummaryResponse secondUserBook = Assert.Single(secondUser.MediaKindSummaries);
        Assert.Equal(MediaKind.Book, secondUserBook.MediaKind);
        Assert.Equal(400d, secondUserBook.TotalConsumedDurationInSeconds, 3);

        Assert.Equal(statistics.Users.Select(summary => summary.UserId).Order().ToArray(), statistics.Users.Select(summary => summary.UserId).ToArray());
        Assert.NotEqual(firstUserLibraryId, secondUserLibraryId);
        AssertTotalsMatchUsers(statistics);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByRegularUser_ShouldReturnForbiddenWithExactShape()
    {
        // Arrange
        HttpClient regularClient = await _apiFactory.CreateAuthenticatedClientAsync();
        string url = "/api/v1/users/consumption-statistics";

        // Act
        HttpResponseMessage response = await regularClient.GetAsync(url);

        // Assert
        await AssertForbiddenAsync(response, url);
        AssertCacheHeaders(response);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.False(problemDetails!.ContainsKey("users"));
        Assert.False(problemDetails.ContainsKey("total"));
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenRestrictedToOneLibrary_ShouldReturnOnlyTheUsersOfThatLibrary()
    {
        // Arrange
        (Guid firstUserId, Guid firstUserLibraryId) = await SeedMusicOnlyUserAsync();
        await SeedBookOnlyUserAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/consumption-statistics?libraryId={firstUserLibraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AllUsersConsumptionStatisticsResponse? statistics = await DeserializeAsync<AllUsersConsumptionStatisticsResponse>(response);
        Assert.NotNull(statistics);
        ConsumptionSummaryResponse user = Assert.Single(statistics!.Users);
        Assert.Equal(firstUserId, user.UserId);
        Assert.Equal(100d, user.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(100d, statistics.Total.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, statistics.Total.CountedSessionCount);
        Assert.Equal(1, statistics.Total.CompletedSessionCount);
        Assert.Equal(1, statistics.Total.StoppedSessionCount);
        MediaKindConsumptionSummaryResponse totalMusic = Assert.Single(statistics.Total.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, totalMusic.MediaKind);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenRestrictedToOneMediaKind_ShouldReturnOnlyThatKindStatistics()
    {
        // Arrange
        await SeedMusicOnlyUserAsync();
        (Guid secondUserId, _) = await SeedBookOnlyUserAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/consumption-statistics?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AllUsersConsumptionStatisticsResponse? statistics = await DeserializeAsync<AllUsersConsumptionStatisticsResponse>(response);
        Assert.NotNull(statistics);
        Assert.NotEmpty(statistics!.Users);
        Assert.All(statistics.Users, summary => Assert.All(summary.MediaKindSummaries, kind => Assert.Equal(MediaKind.MusicTrack, kind.MediaKind)));
        Assert.All(statistics.Total.MediaKindSummaries, kind => Assert.Equal(MediaKind.MusicTrack, kind.MediaKind));
        Assert.DoesNotContain(statistics.Users, summary => summary.UserId == secondUserId);
        AssertTotalsMatchUsers(statistics);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenRestrictedToStartDate_ShouldExcludeEarlierSessions()
    {
        // Arrange
        await SeedMusicOnlyUserAsync();
        (Guid secondUserId, _) = await SeedBookOnlyUserAsync();
        string fromUtc = Uri.EscapeDataString("2026-03-01T00:00:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/consumption-statistics?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AllUsersConsumptionStatisticsResponse? statistics = await DeserializeAsync<AllUsersConsumptionStatisticsResponse>(response);
        Assert.NotNull(statistics);
        // Every book record of this class starts in February, so a March start date can only leave music sessions in the summary.
        Assert.All(statistics!.Users, summary => Assert.All(summary.MediaKindSummaries, kind => Assert.Equal(MediaKind.MusicTrack, kind.MediaKind)));
        Assert.All(statistics.Total.MediaKindSummaries, kind => Assert.Equal(MediaKind.MusicTrack, kind.MediaKind));
        Assert.DoesNotContain(statistics.Users, summary => summary.UserId == secondUserId);
        AssertTotalsMatchUsers(statistics);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenRestrictedToEndDate_ShouldExcludeLaterSessions()
    {
        // Arrange
        (Guid firstUserId, _) = await SeedMusicOnlyUserAsync();
        await SeedBookOnlyUserAsync();
        string toUtc = Uri.EscapeDataString("2026-02-28T23:59:59");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/consumption-statistics?toUtc={toUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AllUsersConsumptionStatisticsResponse? statistics = await DeserializeAsync<AllUsersConsumptionStatisticsResponse>(response);
        Assert.NotNull(statistics);
        // Every music record of this class starts in March, so a February end date can only leave book sessions in the summary.
        Assert.All(statistics!.Users, summary => Assert.All(summary.MediaKindSummaries, kind => Assert.Equal(MediaKind.Book, kind.MediaKind)));
        Assert.All(statistics.Total.MediaKindSummaries, kind => Assert.Equal(MediaKind.Book, kind.MediaKind));
        Assert.DoesNotContain(statistics.Users, summary => summary.UserId == firstUserId);
        AssertTotalsMatchUsers(statistics);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenTargetUserIdQueryParameterIsNotAValidGuid_ShouldIgnoreIt()
    {
        // Arrange
        string url = "/api/v1/users/consumption-statistics?userId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = "/api/v1/users/consumption-statistics";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/consumption-statistics";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/consumption-statistics";
        string url = $"{path}?mediaKind=None";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
    }

    [Theory]
    [InlineData("999")] // a number that is not a member name
    [InlineData("1")] // a number that is a defined enumeration value, which must still be rejected
    [InlineData("not-a-media-kind")] // a name that is not a member of the enumeration
    public async Task GetAllUsersConsumptionStatistics_WhenMediaKindIsNotAMemberName_ShouldReturnValidationProblem(string mediaKind)
    {
        // Arrange
        string path = "/api/v1/users/consumption-statistics";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/consumption-statistics";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
    }

    /// <summary>
    /// Asserts that the combined totals are exactly the sum of the returned user summaries.
    /// </summary>
    /// <param name="statistics">The all users statistics response to assert on.</param>
    private static void AssertTotalsMatchUsers(AllUsersConsumptionStatisticsResponse statistics)
    {
        Assert.Equal(statistics.Users.Sum(summary => summary.TotalConsumedDurationInSeconds), statistics.Total.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(statistics.Users.Sum(summary => summary.CountedSessionCount), statistics.Total.CountedSessionCount);
        Assert.Equal(statistics.Users.Sum(summary => summary.CompletedSessionCount), statistics.Total.CompletedSessionCount);
        Assert.Equal(statistics.Users.Sum(summary => summary.StoppedSessionCount), statistics.Total.StoppedSessionCount);
        double expectedCompletionRate = statistics.Total.CountedSessionCount == 0
            ? 0
            : (double)statistics.Total.CompletedSessionCount / statistics.Total.CountedSessionCount;
        Assert.Equal(expectedCompletionRate, statistics.Total.CompletionRate, 6);

        MediaKind[] expectedMediaKinds = [.. statistics.Users
            .SelectMany(summary => summary.MediaKindSummaries)
            .Select(summary => summary.MediaKind)
            .Distinct()
            .Order()];
        Assert.Equal(expectedMediaKinds, statistics.Total.MediaKindSummaries.Select(summary => summary.MediaKind).ToArray());
        foreach (MediaKindConsumptionSummaryResponse totalKind in statistics.Total.MediaKindSummaries)
        {
            List<MediaKindConsumptionSummaryResponse> userKinds = [.. statistics.Users
                .SelectMany(summary => summary.MediaKindSummaries)
                .Where(summary => summary.MediaKind == totalKind.MediaKind)];
            Assert.Equal(userKinds.Sum(summary => summary.TotalConsumedDurationInSeconds), totalKind.TotalConsumedDurationInSeconds, 3);
            Assert.Equal(userKinds.Sum(summary => summary.CountedSessionCount), totalKind.CountedSessionCount);
            Assert.Equal(userKinds.Sum(summary => summary.CompletedSessionCount), totalKind.CompletedSessionCount);
            Assert.Equal(userKinds.Sum(summary => summary.StoppedSessionCount), totalKind.StoppedSessionCount);
        }
    }

    /// <summary>
    /// Seeds a user with a music library and two counted music sessions.
    /// </summary>
    /// <returns>The Ids of the seeded user and of its music library.</returns>
    private async Task<(Guid userId, Guid libraryId)> SeedMusicOnlyUserAsync()
    {
        Guid userId = await SeedUserAsync($"musicuser_{Guid.NewGuid()}");
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        Guid trackId = Guid.NewGuid();
        await SeedRecordAsync(userId, libraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Completed, s_musicStartedOnUtc, 60_000, 0);
        await SeedRecordAsync(userId, libraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Stopped, s_musicStartedOnUtc.AddMinutes(5), 40_000, 0);
        return (userId, libraryId);
    }

    /// <summary>
    /// Seeds a user with an ebook library and one counted book session and one uncounted one.
    /// </summary>
    /// <returns>The Ids of the seeded user and of its ebook library.</returns>
    private async Task<(Guid userId, Guid libraryId)> SeedBookOnlyUserAsync()
    {
        Guid userId = await SeedUserAsync($"bookuser_{Guid.NewGuid()}");
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.EBook, "Book Library");
        Guid bookId = Guid.NewGuid();
        await SeedRecordAsync(userId, libraryId, MediaKind.Book, bookId, ConsumptionStatus.Completed, s_bookStartedOnUtc, 300_000, 10);
        await SeedRecordAsync(userId, libraryId, MediaKind.Book, bookId, ConsumptionStatus.Stopped, s_bookStartedOnUtc.AddMinutes(5), 100_000, 1);
        return (userId, libraryId);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <param name="username">The username of the seeded user.</param>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedUserAsync(string username)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        UserEntity user = _userEntityFixture.Create(id: userId, username: username, password: "TestPass123!");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return userId;
    }

    /// <summary>
    /// Seeds a media library type owned by <paramref name="userId"/>.
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
    /// <param name="status">The state of the session.</param>
    /// <param name="startedOnUtc">The date and time when the session started.</param>
    /// <param name="consumedDurationMilliseconds">The active duration consumed by the session, in milliseconds.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the session.</param>
    /// <returns>The Id of the seeded record.</returns>
    private async Task<Guid> SeedRecordAsync(
        Guid userId,
        Guid libraryId,
        MediaKind mediaKind,
        Guid mediaItemId,
        ConsumptionStatus status,
        DateTime startedOnUtc,
        long consumedDurationMilliseconds,
        int consecutivePagesRead)
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
            status: status,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: startedOnUtc.AddMinutes(1),
            endedOnUtc: status == ConsumptionStatus.InProgress ? null : startedOnUtc.AddMinutes(2),
            consumedDurationMilliseconds: consumedDurationMilliseconds,
            lastPositionKind: mediaKind == MediaKind.Book ? MediaPositionKind.Location : MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: mediaKind == MediaKind.Book ? null : 0,
            lastPositionLocation: mediaKind == MediaKind.Book ? "chapter-1" : null,
            lastProgressFraction: 0,
            consecutivePagesRead: consecutivePagesRead);
        dbContext.MediaConsumptionRecords.Add(record);
        await dbContext.SaveChangesAsync();
        return recordId;
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
    /// Disposes API factory resources.
    /// </summary>
    /// <remarks>
    /// The authorization rows are removed with the users, so that the next test of the class can create its own admin user without colliding with the
    /// admin role permissions of the previous one.
    /// </remarks>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        await dbContext.Set<RolePermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserPermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserRoleEntity>().ExecuteDeleteAsync();
        await dbContext.MediaConsumptionRecords.ExecuteDeleteAsync();
        await dbContext.Libraries.ExecuteDeleteAsync();
        await dbContext.Set<UserEntity>().ExecuteDeleteAsync();
        await dbContext.Set<RoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<PermissionEntity>().ExecuteDeleteAsync();
        await _apiFactory.RemoveTestUserAsync();
    }
}
