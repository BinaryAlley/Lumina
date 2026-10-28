#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
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
/// Contains integration tests for the <see cref="GetTagConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_trackCompletedStartedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackStoppedStartedOnUtc = new(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookCompletedStartedOnUtc = new(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_uncountedBookStartedOnUtc = new(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc);

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

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTagConsumptionEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetTagConsumption_WhenTaggedItemsWereConsumed_ShouldReturnLowercaseTagsOrderedByItemCountThenDurationThenName()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid musicLibraryId, Guid bookLibraryId, string favouriteTag, string bookClubTag, string rockTag, string ignoredTag) = await SeedTaggedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics/tags");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Equal(3, tags!.Count);
        Assert.All(tags, tag => Assert.Equal(tag.Name.ToLowerInvariant(), tag.Name));
        Assert.Equal(
            new[] { favouriteTag.ToLowerInvariant(), bookClubTag.ToLowerInvariant(), rockTag.ToLowerInvariant() },
            tags.Select(tag => tag.Name).ToArray());

        Assert.Equal(2, tags[0].ConsumedItemCount);
        Assert.Equal(370d, tags[0].ConsumedDurationInSeconds, 3);
        Assert.Equal(1, tags[1].ConsumedItemCount);
        Assert.Equal(300d, tags[1].ConsumedDurationInSeconds, 3);
        Assert.Equal(1, tags[2].ConsumedItemCount);
        Assert.Equal(70d, tags[2].ConsumedDurationInSeconds, 3);
        Assert.DoesNotContain(tags, tag => tag.Name == ignoredTag.ToLowerInvariant());
        Assert.NotEqual(musicLibraryId, bookLibraryId);
    }

    [Fact]
    public async Task GetTagConsumption_WhenRestrictedToOneLibrary_ShouldReturnOnlyThatLibraryTags()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookLibraryId, string favouriteTag, string bookClubTag, _, string ignoredTag) = await SeedTaggedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/tags?libraryId={bookLibraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Equal(
            new[] { bookClubTag.ToLowerInvariant(), favouriteTag.ToLowerInvariant() },
            tags!.Select(tag => tag.Name).ToArray());
        Assert.All(tags, tag => Assert.Equal(1, tag.ConsumedItemCount));
        Assert.All(tags, tag => Assert.Equal(300d, tag.ConsumedDurationInSeconds, 3));
        Assert.DoesNotContain(tags, tag => tag.Name == ignoredTag.ToLowerInvariant());
    }

    [Fact]
    public async Task GetTagConsumption_WhenRestrictedToOneMediaKind_ShouldReturnOnlyThatKindTags()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, string favouriteTag, _, string rockTag, _) = await SeedTaggedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics/tags?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Equal(
            new[] { favouriteTag.ToLowerInvariant(), rockTag.ToLowerInvariant() },
            tags!.Select(tag => tag.Name).ToArray());
        Assert.All(tags, tag => Assert.Equal(1, tag.ConsumedItemCount));
        Assert.All(tags, tag => Assert.Equal(70d, tag.ConsumedDurationInSeconds, 3));
    }

    [Fact]
    public async Task GetTagConsumption_WhenRestrictedToStartDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, string favouriteTag, _, string rockTag, _) = await SeedTaggedConsumptionScenarioAsync(userId);
        string fromUtc = Uri.EscapeDataString("2026-03-01T00:00:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/tags?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Equal(
            new[] { favouriteTag.ToLowerInvariant(), rockTag.ToLowerInvariant() },
            tags!.Select(tag => tag.Name).ToArray());
        Assert.All(tags, tag => Assert.Equal(70d, tag.ConsumedDurationInSeconds, 3));
    }

    [Fact]
    public async Task GetTagConsumption_WhenRestrictedToEndDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, string favouriteTag, string bookClubTag, _, _) = await SeedTaggedConsumptionScenarioAsync(userId);
        string toUtc = Uri.EscapeDataString("2026-02-28T23:59:59");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/tags?toUtc={toUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Equal(
            new[] { bookClubTag.ToLowerInvariant(), favouriteTag.ToLowerInvariant() },
            tags!.Select(tag => tag.Name).ToArray());
        Assert.All(tags, tag => Assert.Equal(1, tag.ConsumedItemCount));
        Assert.All(tags, tag => Assert.Equal(300d, tag.ConsumedDurationInSeconds, 3));
    }

    [Fact]
    public async Task GetTagConsumption_WhenHistoryIsEmpty_ShouldReturnEmptyCollection()
    {
        // Arrange
        _ = GetCurrentUserId();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics/tags");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<TagConsumptionResponse>? tags = await DeserializeAsync<List<TagConsumptionResponse>>(response);
        Assert.NotNull(tags);
        Assert.Empty(tags!);
    }

    [Fact]
    public async Task GetTagConsumption_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = "/api/v1/users/me/consumption-statistics/tags";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetTagConsumption_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/tags";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetTagConsumption_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/tags";
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
    public async Task GetTagConsumption_WhenMediaKindIsNotAMemberName_ShouldReturnValidationProblem(string mediaKind)
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/tags";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetTagConsumption_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/tags";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
    }

    /// <summary>
    /// Seeds a tagged track with a counted and an uncounted session, a tagged book with a counted session, and a tagged book without any counted session.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the libraries and the consumption records.</param>
    /// <returns>
    /// The Ids of the music library and the ebook library, and the unique favourite, book club, rock, and ignored tag names that were seeded.
    /// </returns>
    private async Task<(Guid musicLibraryId, Guid bookLibraryId, string favouriteTag, string bookClubTag, string rockTag, string ignoredTag)> SeedTaggedConsumptionScenarioAsync(Guid userId)
    {
        // Each test seeds its own tag rows, so every name carries a unique suffix to keep the shared Tags table collision free.
        string suffix = Guid.NewGuid().ToString("N");
        string favouriteTag = $"Favourite-{suffix}";
        string bookClubTag = $"Book-Club-{suffix}";
        string rockTag = $"Rock-{suffix}";
        string ignoredTag = $"Ignored-{suffix}";

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // The track and the book share a tag, so the tag rows are resolved once per name, and the whole graph is saved through one context, so the
        // shared row is inserted once instead of once per item that carries it.
        Dictionary<string, TagEntity> tags = [];
        TagEntity ResolveTag(string name)
        {
            if (!tags.TryGetValue(name, out TagEntity? tag))
            {
                tag = _tagEntityFixture.Create(name: name);
                tags[name] = tag;
            }
            return tag;
        }

        Guid musicLibraryId = Guid.NewGuid();
        Guid bookLibraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid uncountedBookId = Guid.NewGuid();

        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: musicLibraryId, userId: userId, title: "Music Library", libraryType: LibraryType.Music, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: bookLibraryId, userId: userId, title: "Book Library", libraryType: LibraryType.EBook, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: musicLibraryId, name: "Queen", includeAlbums: false, includeContributors: false, includeMetadata: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: musicLibraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false));

        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: musicLibraryId, path: $"/music/{trackId:N}.flac", title: "Bohemian Rhapsody", includeMetadata: false);
        track.Tags.Add(ResolveTag(rockTag));
        track.Tags.Add(ResolveTag(favouriteTag));
        dbContext.Tracks.Add(track);

        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: bookLibraryId, path: $"/books/{bookId:N}.epub", title: "The Fellowship of the Ring", includeMetadata: false);
        book.Tags.Add(ResolveTag(favouriteTag));
        book.Tags.Add(ResolveTag(bookClubTag));
        dbContext.Books.Add(book);

        BookEntity uncountedBook = _bookEntityFixture.Create(id: uncountedBookId, libraryId: bookLibraryId, path: $"/books/{uncountedBookId:N}.epub", title: "Uncounted Book", includeMetadata: false);
        uncountedBook.Tags.Add(ResolveTag(ignoredTag));
        dbContext.Books.Add(uncountedBook);

        await dbContext.SaveChangesAsync();

        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Completed, s_trackCompletedStartedOnUtc, 60_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Stopped, s_trackStoppedStartedOnUtc, 10_000, 0);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.Completed, s_bookCompletedStartedOnUtc, 300_000, 10);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, uncountedBookId, ConsumptionStatus.Stopped, s_uncountedBookStartedOnUtc, 500_000, 3);

        return (musicLibraryId, bookLibraryId, favouriteTag, bookClubTag, rockTag, ignoredTag);
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
