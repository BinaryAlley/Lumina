#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// Contains integration tests for the <see cref="GetConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_trackCompletedStartedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackStoppedCountedStartedOnUtc = new(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackStoppedUncountedStartedOnUtc = new(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackInProgressStartedOnUtc = new(2026, 3, 4, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookCompletedStartedOnUtc = new(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookStoppedFivePagesStartedOnUtc = new(2026, 2, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookStoppedSixPagesStartedOnUtc = new(2026, 2, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookInProgressStartedOnUtc = new(2026, 2, 4, 10, 0, 0, DateTimeKind.Utc);

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetConsumptionStatisticsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenHistoryExists_ShouldApplyCountingRulesAndBreakdownByMediaKind()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(userId, summary!.UserId);
        Assert.Equal(359.999, summary.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(4, summary.CountedSessionCount);
        Assert.Equal(2, summary.CompletedSessionCount);
        Assert.Equal(2, summary.StoppedSessionCount);
        Assert.Equal(0.5, summary.CompletionRate, 6);

        Assert.Equal(2, summary.MediaKindSummaries.Count);

        MediaKindConsumptionSummaryResponse trackSummary = summary.MediaKindSummaries[0];
        Assert.Equal(MediaKind.MusicTrack, trackSummary.MediaKind);
        Assert.Equal(79.999, trackSummary.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, trackSummary.CountedSessionCount);
        Assert.Equal(1, trackSummary.CompletedSessionCount);
        Assert.Equal(1, trackSummary.StoppedSessionCount);
        Assert.Equal(0.5, trackSummary.CompletionRate, 6);

        MediaKindConsumptionSummaryResponse bookSummary = summary.MediaKindSummaries[1];
        Assert.Equal(MediaKind.Book, bookSummary.MediaKind);
        Assert.Equal(280d, bookSummary.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, bookSummary.CountedSessionCount);
        Assert.Equal(1, bookSummary.CompletedSessionCount);
        Assert.Equal(1, bookSummary.StoppedSessionCount);
        Assert.Equal(0.5, bookSummary.CompletionRate, 6);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenRestrictedToOneLibrary_ShouldReturnOnlyThatLibraryStatistics()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookLibraryId) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics?libraryId={bookLibraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(280d, summary!.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, summary.CountedSessionCount);
        Assert.Equal(1, summary.CompletedSessionCount);
        Assert.Equal(1, summary.StoppedSessionCount);
        MediaKindConsumptionSummaryResponse bookSummary = Assert.Single(summary.MediaKindSummaries);
        Assert.Equal(MediaKind.Book, bookSummary.MediaKind);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenRestrictedToOneMediaKind_ShouldReturnOnlyThatKindStatistics()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(79.999, summary!.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, summary.CountedSessionCount);
        MediaKindConsumptionSummaryResponse trackSummary = Assert.Single(summary.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, trackSummary.MediaKind);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenRestrictedToStartDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        await SeedConsumptionScenarioAsync(userId);
        string fromUtc = Uri.EscapeDataString("2026-03-01T00:00:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(79.999, summary!.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, summary.CountedSessionCount);
        MediaKindConsumptionSummaryResponse trackSummary = Assert.Single(summary.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, trackSummary.MediaKind);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenStartDateCarriesAZoneDesignator_ShouldUseTheDescribedUtcInstant()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        await SeedConsumptionScenarioAsync(userId);
        string fromUtc = Uri.EscapeDataString("2026-03-01T00:00:00Z");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(79.999, summary!.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, summary.CountedSessionCount);
        MediaKindConsumptionSummaryResponse trackSummary = Assert.Single(summary.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, trackSummary.MediaKind);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenRestrictedToEndDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        await SeedConsumptionScenarioAsync(userId);
        string toUtc = Uri.EscapeDataString("2026-02-28T23:59:59");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics?toUtc={toUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(280d, summary!.TotalConsumedDurationInSeconds, 3);
        Assert.Equal(2, summary.CountedSessionCount);
        MediaKindConsumptionSummaryResponse bookSummary = Assert.Single(summary.MediaKindSummaries);
        Assert.Equal(MediaKind.Book, bookSummary.MediaKind);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenHistoryIsEmpty_ShouldReturnZeroedTotalsAndEmptyBreakdown()
    {
        // Arrange
        Guid userId = GetCurrentUserId();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(userId, summary!.UserId);
        Assert.Equal(0d, summary.TotalConsumedDurationInSeconds);
        Assert.Equal(0, summary.CountedSessionCount);
        Assert.Equal(0, summary.CompletedSessionCount);
        Assert.Equal(0, summary.StoppedSessionCount);
        Assert.Equal(0d, summary.CompletionRate);
        Assert.Empty(summary.MediaKindSummaries);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenInProgressSessionReachedTheThreshold_ShouldCountItTowardsStatistics()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        Guid trackId = Guid.NewGuid();
        // The media kind rule counts a session when it completed or when it reached the threshold of its kind, which an in-progress session can do.
        await SeedRecordAsync(userId, libraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.InProgress, s_trackCompletedStartedOnUtc, 30_000, 0);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ConsumptionSummaryResponse? summary = await DeserializeAsync<ConsumptionSummaryResponse>(response);
        Assert.NotNull(summary);
        Assert.Equal(1, summary!.CountedSessionCount);
        Assert.Equal(0, summary.CompletedSessionCount);
        Assert.Equal(0, summary.StoppedSessionCount);
        Assert.Equal(30d, summary.TotalConsumedDurationInSeconds, 3);
        MediaKindConsumptionSummaryResponse trackSummary = Assert.Single(summary.MediaKindSummaries);
        Assert.Equal(1, trackSummary.CountedSessionCount);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = "/api/v1/users/me/consumption-statistics";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics";
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
    public async Task GetConsumptionStatistics_WhenMediaKindIsNotAMemberName_ShouldReturnValidationProblem(string mediaKind)
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
    }

    [Theory]
    [InlineData("not-a-date")] // a textual value that is not a date
    [InlineData("2026-13-45T99:99:99")] // a value with out of range components
    public async Task GetConsumptionStatistics_WhenADateBoundIsNotAValidDate_ShouldReturnValidationProblem(string invalidDate)
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics";
        string url = $"{path}?fromUtc={Uri.EscapeDataString(invalidDate)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
        AssertCacheHeaders(response);
    }

    /// <summary>
    /// Seeds a music library and an ebook library owned by <paramref name="userId"/>, with one track and one book history.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the libraries and the consumption records.</param>
    /// <returns>The Ids of the music library and the ebook library.</returns>
    private async Task<(Guid musicLibraryId, Guid bookLibraryId)> SeedConsumptionScenarioAsync(Guid userId)
    {
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        Guid bookLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook, "Book Library");
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();

        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Completed, s_trackCompletedStartedOnUtc, 10_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Stopped, s_trackStoppedCountedStartedOnUtc, 30_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Stopped, s_trackStoppedUncountedStartedOnUtc, 29_999, 0);
        // An in-progress session counts only through the threshold branch of its media kind rule, so it is kept below both thresholds here.
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.InProgress, s_trackInProgressStartedOnUtc, 10_000, 0);

        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.Completed, s_bookCompletedStartedOnUtc, 100_000, 0);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.Stopped, s_bookStoppedFivePagesStartedOnUtc, 60_000, 5);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.Stopped, s_bookStoppedSixPagesStartedOnUtc, 90_000, 6);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.InProgress, s_bookInProgressStartedOnUtc, 30_000, 2);

        return (musicLibraryId, bookLibraryId);
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
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
