#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetTagConsumption;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// Contains integration tests for the <see cref="GetUserTagConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserTagConsumptionEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_startedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserTagConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetUserTagConsumptionEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetUserTagConsumption_WhenCurrentUserNamesTheirOwnId_ShouldReturnTheirTagStatistics()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        string firstTag = $"First-{Guid.NewGuid():N}";
        string secondTag = $"Second-{Guid.NewGuid():N}";
        Guid trackId = await SeedTrackAsync(libraryId, "Bohemian Rhapsody", "A Night at the Opera", "Queen", [firstTag, secondTag]);
        await SeedRecordAsync(userId, libraryId, MediaKind.MusicTrack, trackId, 60_000, 0);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/{userId}/consumption-statistics/tags");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Equal(2, tags!.Count);
        Assert.Equal(
            new[] { firstTag.ToLowerInvariant(), secondTag.ToLowerInvariant() },
            tags.Select(tag => tag.Name).ToArray());
        Assert.All(tags, tag => Assert.Equal(1, tag.ConsumedItemCount));
        Assert.All(tags, tag => Assert.Equal(60d, tag.ConsumedDurationInSeconds, 3));
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenRegularUserNamesAnotherUser_ShouldReturnForbiddenAndLeakNoData()
    {
        // Arrange
        Guid otherUserId = await SeedUserAsync($"otheruser_{Guid.NewGuid()}");
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId, LibraryType.Music, "Other Library");
        string secretTag = $"Secret-{Guid.NewGuid():N}";
        Guid otherTrackId = await SeedTrackAsync(otherLibraryId, "Secret Track", "Secret Album", "Secret Artist", [secretTag]);
        await SeedRecordAsync(otherUserId, otherLibraryId, MediaKind.MusicTrack, otherTrackId, 60_000, 0);
        string url = $"/api/v1/users/{otherUserId}/consumption-statistics/tags";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertForbiddenAsync(response, url);
        AssertCacheHeaders(response);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(secretTag.ToLowerInvariant(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Secret Track", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenAdminNamesAnotherUser_ShouldReturnThatUsersTagStatistics()
    {
        // Arrange
        Guid targetUserId = await SeedUserAsync($"targetuser_{Guid.NewGuid()}");
        Guid targetLibraryId = await SeedLibraryAsync(targetUserId, LibraryType.EBook, "Target Book Library");
        string targetTag = $"Target-{Guid.NewGuid():N}";
        Guid targetBookId = await SeedBookAsync(targetLibraryId, "Target Book", [targetTag]);
        await SeedRecordAsync(targetUserId, targetLibraryId, MediaKind.Book, targetBookId, 300_000, 10);
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();

        // Act
        HttpResponseMessage response = await adminClient.GetAsync($"/api/v1/users/{targetUserId}/consumption-statistics/tags");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        TagConsumptionResponse tag = Assert.Single(tags!);
        Assert.Equal(targetTag.ToLowerInvariant(), tag.Name);
        Assert.Equal(1, tag.ConsumedItemCount);
        Assert.Equal(300d, tag.ConsumedDurationInSeconds, 3);
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenUserIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string url = "/api/v1/users/not-a-guid/consumption-statistics/tags";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "TargetUserIdIsInvalid");
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/users/{Guid.NewGuid()}/consumption-statistics/tags";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string path = $"/api/v1/users/{userId}/consumption-statistics/tags";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string path = $"/api/v1/users/{userId}/consumption-statistics/tags";
        string url = $"{path}?mediaKind=None";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
    }

    [Fact]
    public async Task GetUserTagConsumption_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string path = $"/api/v1/users/{userId}/consumption-statistics/tags";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
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
    /// Seeds a tagged track of <paramref name="libraryId"/>, together with its artist and album.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the track.</param>
    /// <param name="trackTitle">The title of the track.</param>
    /// <param name="albumTitle">The title of the album that contains the track.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="tagNames">The tag names carried by the track.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, string trackTitle, string albumTitle, string artistName, IReadOnlyList<string> tagNames)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: artistName, includeAlbums: false, includeContributors: false, includeMetadata: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: albumTitle, includeTracks: false, includeMetadata: false));
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: $"/music/{trackId:N}.flac", title: trackTitle, includeMetadata: false);
        foreach (string tagName in tagNames)
            track.Tags.Add(_tagEntityFixture.Create(name: tagName));
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        return trackId;
    }

    /// <summary>
    /// Seeds a tagged book of <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <param name="tagNames">The tag names carried by the book.</param>
    /// <returns>The Id of the seeded book.</returns>
    private async Task<Guid> SeedBookAsync(Guid libraryId, string bookTitle, IReadOnlyList<string> tagNames)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid bookId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: bookTitle, includeMetadata: false);
        foreach (string tagName in tagNames)
            book.Tags.Add(_tagEntityFixture.Create(name: tagName));
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return bookId;
    }

    /// <summary>
    /// Seeds a completed consumption record of a media item for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the record.</param>
    /// <param name="libraryId">The Id of the library that contains the media item.</param>
    /// <param name="mediaKind">The kind of the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="consumedDurationMilliseconds">The active duration consumed by the session, in milliseconds.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the session.</param>
    /// <returns>The Id of the seeded record.</returns>
    private async Task<Guid> SeedRecordAsync(
        Guid userId,
        Guid libraryId,
        MediaKind mediaKind,
        Guid mediaItemId,
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
            status: ConsumptionStatus.Completed,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_startedOnUtc.AddMinutes(1),
            endedOnUtc: s_startedOnUtc.AddMinutes(2),
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
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
