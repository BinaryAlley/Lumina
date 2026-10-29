#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetMostConsumed;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.GetMostConsumed;

/// <summary>
/// Contains integration tests for the <see cref="GetMostConsumedEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_trackAFirstStartedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackASecondStartedOnUtc = new(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackAThirdStartedOnUtc = new(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackBFirstStartedOnUtc = new(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackBSecondStartedOnUtc = new(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookFirstStartedOnUtc = new(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookSecondStartedOnUtc = new(2026, 2, 2, 10, 0, 0, DateTimeKind.Utc);

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
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetMostConsumedEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetMostConsumed_WhenHistoryExists_ShouldOrderByCountedSessionsThenDurationThenLastConsumed()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid musicLibraryId, Guid trackAId, Guid trackBId, Guid bookLibraryId, Guid bookCId) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/most-consumed?currentPage=1&perPage=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        Assert.Equal(1, history!.CurrentPage);
        Assert.Equal(10, history.PerPage);
        Assert.Equal(3, history.Count);
        Assert.Equal(1, history.NumberOfPages);
        Assert.Equal(3, history.Data.Count);

        Assert.Equal(new[] { trackAId, trackBId, bookCId }, history.Data.Select(item => item.MediaItemId).ToArray());

        ConsumedItemResponse mostConsumed = history.Data[0];
        Assert.Equal(MediaKind.MusicTrack, mostConsumed.MediaKind);
        Assert.Equal(musicLibraryId, mostConsumed.LibraryId);
        Assert.True(mostConsumed.IsAvailable);
        Assert.Equal("Track A", mostConsumed.Title);
        Assert.Equal("Album A", mostConsumed.CollectionTitle);
        Assert.Equal("Artist A", mostConsumed.CreatorName);
        Assert.Null(mostConsumed.CoverPath);
        Assert.Equal(s_trackAThirdStartedOnUtc, mostConsumed.LastConsumedOnUtc);
        Assert.Equal(3, mostConsumed.CountedSessionCount);
        Assert.Equal(3, mostConsumed.CompletedSessionCount);
        Assert.Equal(30d, mostConsumed.ConsumedDurationInSeconds);

        ConsumedItemResponse second = history.Data[1];
        Assert.Equal(trackBId, second.MediaItemId);
        Assert.Equal(2, second.CountedSessionCount);
        Assert.Equal(200d, second.ConsumedDurationInSeconds);
        Assert.Equal(s_trackBSecondStartedOnUtc, second.LastConsumedOnUtc);

        ConsumedItemResponse third = history.Data[2];
        Assert.Equal(bookCId, third.MediaItemId);
        Assert.Equal(bookLibraryId, third.LibraryId);
        Assert.Equal(2, third.CountedSessionCount);
        Assert.Equal(200d, third.ConsumedDurationInSeconds);
        Assert.Equal(s_bookSecondStartedOnUtc, third.LastConsumedOnUtc);
    }

    [Fact]
    public async Task GetMostConsumed_WhenRestrictedToOneLibrary_ShouldReturnOnlyThatLibraryItems()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, _, Guid bookLibraryId, Guid bookCId) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-history/most-consumed?libraryId={bookLibraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(bookCId, item.MediaItemId);
        Assert.Equal(bookLibraryId, item.LibraryId);
        Assert.Equal(1, history.Count);
    }

    [Fact]
    public async Task GetMostConsumed_WhenRestrictedToOneMediaKind_ShouldReturnOnlyThatKind()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid trackAId, Guid trackBId, _, _) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/most-consumed?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        Assert.All(history!.Data, item => Assert.Equal(MediaKind.MusicTrack, item.MediaKind));
        Assert.Equal(new[] { trackAId, trackBId }, history.Data.Select(item => item.MediaItemId).ToArray());
        Assert.Equal(2, history.Count);
    }

    [Fact]
    public async Task GetMostConsumed_WhenRestrictedToStartDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid trackAId, Guid trackBId, _, _) = await SeedConsumptionScenarioAsync(userId);
        string fromUtc = Uri.EscapeDataString("2026-03-02T00:00:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-history/most-consumed?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        Assert.Equal(new[] { trackBId, trackAId }, history!.Data.Select(item => item.MediaItemId).ToArray());
        Assert.Equal(2, history.Count);
        Assert.Equal(2, history.Data[0].CountedSessionCount);
        Assert.Equal(200d, history.Data[0].ConsumedDurationInSeconds);
        Assert.Equal(2, history.Data[1].CountedSessionCount);
        Assert.Equal(20d, history.Data[1].ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task GetMostConsumed_WhenRestrictedToEndDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid trackAId, Guid trackBId, _, Guid bookCId) = await SeedConsumptionScenarioAsync(userId);
        string toUtc = Uri.EscapeDataString("2026-03-02T08:30:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-history/most-consumed?toUtc={toUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        // The end of the range keeps the February book sessions and drops the later track sessions, which changes both the counts and the order.
        Assert.Equal(new[] { bookCId, trackBId, trackAId }, history!.Data.Select(item => item.MediaItemId).ToArray());
        Assert.Equal(2, history.Data[0].CountedSessionCount);
        Assert.Equal(200d, history.Data[0].ConsumedDurationInSeconds);
        Assert.Equal(1, history.Data[1].CountedSessionCount);
        Assert.Equal(100d, history.Data[1].ConsumedDurationInSeconds);
        Assert.Equal(s_trackBFirstStartedOnUtc, history.Data[1].LastConsumedOnUtc);
        Assert.Equal(1, history.Data[2].CountedSessionCount);
        Assert.Equal(10d, history.Data[2].ConsumedDurationInSeconds);
        Assert.Equal(s_trackAFirstStartedOnUtc, history.Data[2].LastConsumedOnUtc);
    }

    [Fact]
    public async Task GetMostConsumed_WhenPaginationIsRequested_ShouldReturnRequestedPage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, _, _, Guid bookCId) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/most-consumed?currentPage=2&perPage=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        Assert.Equal(2, history!.CurrentPage);
        Assert.Equal(2, history.PerPage);
        Assert.Equal(3, history.Count);
        Assert.Equal(2, history.NumberOfPages);
        ConsumedItemResponse item = Assert.Single(history.Data);
        Assert.Equal(bookCId, item.MediaItemId);
    }

    [Fact]
    public async Task GetMostConsumed_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = "/api/v1/users/me/consumption-history/most-consumed";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetMostConsumed_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/most-consumed";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetMostConsumed_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/most-consumed";
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
    public async Task GetMostConsumed_WhenMediaKindIsNotAMemberName_ShouldReturnValidationProblem(string mediaKind)
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/most-consumed";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetMostConsumed_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/most-consumed";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
    }

    /// <summary>
    /// Seeds two tracks with three and two sessions and a book with two sessions, all owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the libraries and the consumption records.</param>
    /// <returns>The Ids of the music library, both tracks, the ebook library, and the book.</returns>
    private async Task<(Guid musicLibraryId, Guid trackAId, Guid trackBId, Guid bookLibraryId, Guid bookCId)> SeedConsumptionScenarioAsync(Guid userId)
    {
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        Guid trackAId = await SeedTrackAsync(musicLibraryId, "Track A", "Album A", "Artist A");
        Guid trackBId = await SeedTrackAsync(musicLibraryId, "Track B", "Album B", "Artist B");
        Guid bookLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook, "Book Library");
        // The contributor display name is unique across the whole database, so every test seeds its own spelling of the author.
        Guid bookCId = await SeedBookAsync(bookLibraryId, "Book C", $"Author C {Guid.NewGuid():N}");

        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackAId, s_trackAFirstStartedOnUtc, 10_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackAId, s_trackASecondStartedOnUtc, 10_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackAId, s_trackAThirdStartedOnUtc, 10_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackBId, s_trackBFirstStartedOnUtc, 100_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackBId, s_trackBSecondStartedOnUtc, 100_000, 0);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookCId, s_bookFirstStartedOnUtc, 100_000, 10);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookCId, s_bookSecondStartedOnUtc, 100_000, 10);

        return (musicLibraryId, trackAId, trackBId, bookLibraryId, bookCId);
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
    /// Seeds a track of <paramref name="libraryId"/>, together with its artist and album.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the track.</param>
    /// <param name="trackTitle">The title of the track.</param>
    /// <param name="albumTitle">The title of the album that contains the track.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, string trackTitle, string albumTitle, string artistName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: artistName, includeAlbums: false, includeContributors: false, includeMetadata: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: albumTitle, includeTracks: false, includeMetadata: false));
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: $"/music/{trackId:N}.flac", title: trackTitle, includeMetadata: false);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        return trackId;
    }

    /// <summary>
    /// Seeds a book of <paramref name="libraryId"/>, together with an author contributor when a name is supplied.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <param name="authorName">The name of the author of the book, or <see langword="null"/> for no contributor.</param>
    /// <returns>The Id of the seeded book.</returns>
    private async Task<Guid> SeedBookAsync(Guid libraryId, string bookTitle, string? authorName = null)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid bookId = Guid.NewGuid();
        Guid? contributorId = null;
        if (authorName is not null)
        {
            contributorId = Guid.NewGuid();
            dbContext.MediaContributors.Add(_mediaContributorEntityFixture.Create(id: contributorId.Value, displayName: authorName));
            await dbContext.SaveChangesAsync();
        }
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: bookTitle, includeMetadata: false);
        if (contributorId is not null)
            book.Contributors.Add(_bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: contributorId.Value, role: MediaContributorRole.Author));
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
    /// <param name="startedOnUtc">The date and time when the session started.</param>
    /// <param name="consumedDurationMilliseconds">The active duration consumed by the session, in milliseconds.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the session.</param>
    /// <returns>The Id of the seeded record.</returns>
    private async Task<Guid> SeedRecordAsync(
        Guid userId,
        Guid libraryId,
        MediaKind mediaKind,
        Guid mediaItemId,
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
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: startedOnUtc,
            endedOnUtc: startedOnUtc.AddMinutes(1),
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
