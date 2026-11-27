#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetConsumptionRecord;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.MediaConsumption.GetConsumptionRecord;

/// <summary>
/// Contains integration tests for the <see cref="GetConsumptionRecordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetConsumptionRecordEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetConsumptionRecord_WhenRecordBelongsToCurrentUser_ShouldReturnRecordDetails()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid mediaItemId, Guid recordId) = await SeedBookRecordAsync(userId, consumedDurationMilliseconds: 90_500, lastProgressFraction: 0.42, consecutivePagesRead: 12, lastPositionLocation: "chapter-3");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-records/{recordId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(recordId, record!.Id);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(mediaItemId, record.MediaItemId);
        Assert.Equal(MediaKind.Book, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(s_startedOnUtc, record.StartedOnUtc);
        Assert.Equal(s_lastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Null(record.EndedOnUtc);
        Assert.Equal(90.5, record.ConsumedDurationInSeconds);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.Location, record.LastPosition!.Kind);
        Assert.Null(record.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-3", record.LastPosition.Location);
        Assert.Equal(0.42, record.LastProgressFraction);
        Assert.Equal(12, record.ConsecutivePagesRead);
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenRecordBelongsToAnotherUser_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (_, _, Guid recordId) = await SeedBookRecordAsync(otherUserId, consumedDurationMilliseconds: 30_000, lastProgressFraction: 0.2, consecutivePagesRead: 2, lastPositionLocation: "chapter-1");
        string url = $"/api/v1/users/me/consumption-records/{recordId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "ConsumptionRecordNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenRecordDoesNotExist_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "ConsumptionRecordNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenRecordIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = "/api/v1/users/me/consumption-records/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "ConsumptionRecordIdCannotBeEmpty");
    }

    [Fact]
    public async Task GetConsumptionRecord_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/users/me/consumption-records/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    /// <summary>
    /// Seeds a book library, a book, and an in-progress consumption record of the book for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library and the record.</param>
    /// <param name="consumedDurationMilliseconds">The consumed duration of the record, in milliseconds.</param>
    /// <param name="lastProgressFraction">The last reported progress fraction of the record.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the record.</param>
    /// <param name="lastPositionLocation">The last known reading location of the record.</param>
    /// <returns>The Ids of the seeded library, book and consumption record.</returns>
    private async Task<(Guid libraryId, Guid mediaItemId, Guid recordId)> SeedBookRecordAsync(Guid userId, long consumedDurationMilliseconds, double lastProgressFraction, int consecutivePagesRead, string lastPositionLocation)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        Guid recordId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
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
            consecutivePagesRead: consecutivePagesRead));
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
