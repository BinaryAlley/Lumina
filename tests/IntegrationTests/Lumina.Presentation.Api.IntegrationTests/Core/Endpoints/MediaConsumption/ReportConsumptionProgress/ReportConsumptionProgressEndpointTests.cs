#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// Contains integration tests for the <see cref="ReportConsumptionProgressEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_startedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 3, 1, 10, 5, 0, DateTimeKind.Utc);

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly ReportConsumptionProgressRequestFixture _reportConsumptionProgressRequestFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ReportConsumptionProgressEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ReportConsumptionProgress_WhenRecordIsInProgress_ShouldUpdateProgress()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid mediaItemId, Guid recordId) = await SeedMusicTrackRecordAsync(userId, status: ConsumptionStatus.InProgress, consumedDurationMilliseconds: 0, lastProgressFraction: 0.1, lastPositionKind: MediaPositionKind.TimeOffset, lastPositionTimeOffsetMilliseconds: 5_000);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: nameof(MediaPositionKind.TimeOffset), timeOffsetSeconds: 12.5, includeLocation: false),
            progressFraction: 0.35,
            consumedDurationInSeconds: 45);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(recordId, record!.Id);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(mediaItemId, record.MediaItemId);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Null(record.EndedOnUtc);
        Assert.Equal(45.0, record.ConsumedDurationInSeconds);
        Assert.Equal(0.35, record.LastProgressFraction);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, record.LastPosition!.Kind);
        Assert.Equal(12.5, record.LastPosition.TimeOffsetSeconds);
        Assert.Null(record.LastPosition.Location);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(stored => stored.Id == recordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(ConsumptionStatus.InProgress, storedRecord!.Status);
        Assert.Null(storedRecord.EndedOnUtc);
        Assert.Equal(45_000, storedRecord.ConsumedDurationMilliseconds);
        Assert.Equal(0.35, storedRecord.LastProgressFraction);
        Assert.Equal(MediaPositionKind.TimeOffset, storedRecord.LastPositionKind);
        Assert.Equal(12_500, storedRecord.LastPositionTimeOffsetMilliseconds);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenBookRecordAndLocationPosition_ShouldStoreLocation()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid mediaItemId, Guid recordId) = await SeedBookRecordAsync(userId, consumedDurationMilliseconds: 0, lastProgressFraction: 0.2, lastPositionLocation: "chapter-1");
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: nameof(MediaPositionKind.Location), location: "chapter-2", includeTimeOffsetSeconds: false),
            progressFraction: 0.5,
            consumedDurationInSeconds: 60);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(libraryId, record!.LibraryId);
        Assert.Equal(mediaItemId, record.MediaItemId);
        Assert.Equal(MediaKind.Book, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(60.0, record.ConsumedDurationInSeconds);
        Assert.Equal(0.5, record.LastProgressFraction);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.Location, record.LastPosition!.Kind);
        Assert.Null(record.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-2", record.LastPosition.Location);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(stored => stored.Id == recordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(MediaPositionKind.Location, storedRecord!.LastPositionKind);
        Assert.Equal("chapter-2", storedRecord.LastPositionLocation);
        Assert.Null(storedRecord.LastPositionTimeOffsetMilliseconds);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenRecordIsCompleted_ShouldReturnConsumptionIsNotInProgress()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, Guid recordId) = await SeedMusicTrackRecordAsync(userId, status: ConsumptionStatus.Completed, consumedDurationMilliseconds: 60_000, lastProgressFraction: 1);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 0.9, consumedDurationInSeconds: 70);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Conflict, "General.Conflict", "ConsumptionIsNotInProgress", url, "https://tools.ietf.org/html/rfc9110#section-15.5.10");
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenRecordDoesNotExist_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 0.5, consumedDurationInSeconds: 10);
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "ConsumptionRecordNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenRecordBelongsToAnotherUser_ShouldReturnConsumptionRecordNotFoundAndLeaveRecordUnchanged()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (_, _, Guid recordId) = await SeedMusicTrackRecordAsync(otherUserId, status: ConsumptionStatus.InProgress, consumedDurationMilliseconds: 15_000, lastProgressFraction: 0.2, lastPositionKind: MediaPositionKind.TimeOffset, lastPositionTimeOffsetMilliseconds: 15_000);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: _mediaPositionRequestFixture.Create(kind: nameof(MediaPositionKind.TimeOffset), timeOffsetSeconds: 90, includeLocation: false),
            progressFraction: 0.9,
            consumedDurationInSeconds: 90);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "ConsumptionRecordNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(stored => stored.Id == recordId);
        Assert.NotNull(storedRecord);
        Assert.Equal(ConsumptionStatus.InProgress, storedRecord!.Status);
        Assert.Equal(15_000, storedRecord.ConsumedDurationMilliseconds);
        Assert.Equal(0.2, storedRecord.LastProgressFraction);
        Assert.Equal(s_lastActivityOnUtc, storedRecord.LastActivityOnUtc);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenConsecutivePagesReadIsPositiveForNonBook_ShouldReturnForbidden()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, Guid recordId) = await SeedMusicTrackRecordAsync(userId, status: ConsumptionStatus.InProgress, consumedDurationMilliseconds: 0, lastProgressFraction: 0.1);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 0.2, consumedDurationInSeconds: 30, consecutivePagesRead: 3);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Forbidden", "ConsecutivePagesReadIsOnlySupportedForPageBasedMedia", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenProgressFractionIsOutOfRange_ShouldReturnInvalidProgressFraction()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, Guid recordId) = await SeedMusicTrackRecordAsync(userId, status: ConsumptionStatus.InProgress, consumedDurationMilliseconds: 0, lastProgressFraction: 0.1);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 1.5, consumedDurationInSeconds: 30);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "InvalidProgressFraction");
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenConsumedDurationIsLowerThanStored_ShouldReturnConsumedDurationCannotDecrease()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, Guid recordId) = await SeedMusicTrackRecordAsync(userId, status: ConsumptionStatus.InProgress, consumedDurationMilliseconds: 100_000, lastProgressFraction: 0.3);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 0.4, consumedDurationInSeconds: 50);
        string url = $"/api/v1/users/me/consumption-records/{recordId}/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Forbidden", "ConsumedDurationCannotDecrease", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenRecordIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 0.5, consumedDurationInSeconds: 10);
        string url = "/api/v1/users/me/consumption-records/not-a-guid/progress";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "ConsumptionRecordIdCannotBeEmpty");
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false, progressFraction: 0.5, consumedDurationInSeconds: 10);
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}/progress";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync(url, request);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    /// <summary>
    /// Seeds a music library and an in-progress consumption record of a music track for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library and the record.</param>
    /// <param name="status">The state of the seeded consumption session.</param>
    /// <param name="consumedDurationMilliseconds">The consumed duration of the record, in milliseconds.</param>
    /// <param name="lastProgressFraction">The last reported progress fraction of the record.</param>
    /// <param name="lastPositionKind">The kind of the last known position, or <see langword="null"/> when the record has no known position.</param>
    /// <param name="lastPositionTimeOffsetMilliseconds">The last known position, in milliseconds, when it is a time offset.</param>
    /// <returns>The Ids of the seeded library, media item and consumption record.</returns>
    private async Task<(Guid libraryId, Guid mediaItemId, Guid recordId)> SeedMusicTrackRecordAsync(
        Guid userId,
        ConsumptionStatus status,
        long consumedDurationMilliseconds,
        double lastProgressFraction,
        MediaPositionKind? lastPositionKind = null,
        long? lastPositionTimeOffsetMilliseconds = null)
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
            lastPositionKind: lastPositionKind,
            lastPositionTimeOffsetMilliseconds: lastPositionTimeOffsetMilliseconds,
            lastProgressFraction: lastProgressFraction,
            consecutivePagesRead: 0,
            includeLastPositionKind: lastPositionKind.HasValue,
            includeLastPositionTimeOffsetMilliseconds: lastPositionKind.HasValue,
            includeLastPositionLocation: lastPositionKind.HasValue));
        await dbContext.SaveChangesAsync();
        return (libraryId, mediaItemId, recordId);
    }

    /// <summary>
    /// Seeds an ebook library and an in-progress consumption record of a book for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library and the record.</param>
    /// <param name="consumedDurationMilliseconds">The consumed duration of the record, in milliseconds.</param>
    /// <param name="lastProgressFraction">The last reported progress fraction of the record.</param>
    /// <param name="lastPositionLocation">The last known reading location of the record.</param>
    /// <returns>The Ids of the seeded library, book and consumption record.</returns>
    private async Task<(Guid libraryId, Guid mediaItemId, Guid recordId)> SeedBookRecordAsync(Guid userId, long consumedDurationMilliseconds, double lastProgressFraction, string lastPositionLocation)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        Guid recordId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Book Library", libraryType: LibraryType.EBook, contentLocations: []));
        dbContext.Books.Add(_bookEntityFixture.Create(id: mediaItemId, libraryId: libraryId, path: $"/books/{mediaItemId:N}.epub", title: "Test Book", includeMetadata: false));
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            consumedDurationMilliseconds: consumedDurationMilliseconds,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: lastPositionLocation,
            lastProgressFraction: lastProgressFraction,
            consecutivePagesRead: 5));
        await dbContext.SaveChangesAsync();
        return (libraryId, mediaItemId, recordId);
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
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
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
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
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
    private async Task AssertProblemDetails(HttpResponseMessage response, HttpStatusCode statusCode, string expectedTitle, string expectedDetail, string expectedInstance, string expectedType)
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
