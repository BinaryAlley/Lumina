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
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetRecentlyConsumed;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.GetRecentlyConsumed;

/// <summary>
/// Contains integration tests for the <see cref="GetRecentlyConsumedEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyConsumedEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_trackFirstStartedOnUtc = new(2026, 3, 1, 9, 55, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackFirstLastActivityOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackSecondStartedOnUtc = new(2026, 3, 2, 9, 55, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackSecondLastActivityOnUtc = new(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackThirdStartedOnUtc = new(2026, 3, 3, 9, 55, 0, DateTimeKind.Utc);
    private static readonly DateTime s_trackThirdLastActivityOnUtc = new(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookStartedOnUtc = new(2026, 2, 28, 19, 50, 0, DateTimeKind.Utc);
    private static readonly DateTime s_bookLastActivityOnUtc = new(2026, 2, 28, 20, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_missingItemLastActivityOnUtc = new(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);

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
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyConsumedEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetRecentlyConsumedEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetRecentlyConsumed_WhenCalledForCurrentUser_ShouldReturnAggregatedHistory()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid musicLibraryId, Guid trackId, Guid bookLibraryId, Guid bookId, string authorName) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/recently-consumed?currentPage=1&perPage=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        Assert.Equal(1, history!.CurrentPage);
        Assert.Equal(10, history.PerPage);
        Assert.Equal(2, history.Count);
        Assert.Equal(1, history.NumberOfPages);
        Assert.Equal(2, history.Data.Count);

        ConsumedItemResponse track = history.Data[0];
        Assert.Equal(MediaKind.MusicTrack, track.MediaKind);
        Assert.Equal(trackId, track.MediaItemId);
        Assert.Equal(musicLibraryId, track.LibraryId);
        Assert.True(track.IsAvailable);
        Assert.Equal("Bohemian Rhapsody", track.Title);
        Assert.Equal("A Night at the Opera", track.CollectionTitle);
        Assert.Equal("Queen", track.CreatorName);
        Assert.Null(track.CoverPath);
        Assert.Equal(s_trackThirdLastActivityOnUtc, track.LastConsumedOnUtc);
        Assert.Equal(2, track.CountedSessionCount);
        Assert.Equal(1, track.CompletedSessionCount);
        Assert.Equal(100d, track.ConsumedDurationInSeconds);

        ConsumedItemResponse book = history.Data[1];
        Assert.Equal(MediaKind.Book, book.MediaKind);
        Assert.Equal(bookId, book.MediaItemId);
        Assert.Equal(bookLibraryId, book.LibraryId);
        Assert.True(book.IsAvailable);
        Assert.Equal("The Fellowship of the Ring", book.Title);
        Assert.Equal(string.Empty, book.CollectionTitle);
        Assert.Equal(authorName, book.CreatorName);
        Assert.Null(book.CoverPath);
        Assert.Equal(s_bookLastActivityOnUtc, book.LastConsumedOnUtc);
        Assert.Equal(1, book.CountedSessionCount);
        Assert.Equal(1, book.CompletedSessionCount);
        Assert.Equal(500d, book.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenRestrictedToOneLibrary_ShouldReturnOnlyThatLibraryItems()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, Guid bookLibraryId, Guid bookId, _) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-history/recently-consumed?libraryId={bookLibraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(bookId, item.MediaItemId);
        Assert.Equal(bookLibraryId, item.LibraryId);
        Assert.Equal(1, history.Count);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenRestrictedToOneMediaKind_ShouldReturnOnlyThatKind()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid musicLibraryId, Guid trackId, _, _, _) = await SeedConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/recently-consumed?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(MediaKind.MusicTrack, item.MediaKind);
        Assert.Equal(trackId, item.MediaItemId);
        Assert.Equal(musicLibraryId, item.LibraryId);
        Assert.Equal(1, history.Count);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenRestrictedToStartDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid trackId, _, _, _) = await SeedConsumptionScenarioAsync(userId);
        string fromUtc = Uri.EscapeDataString("2026-03-01T00:00:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-history/recently-consumed?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(trackId, item.MediaItemId);
        Assert.Equal(2, item.CountedSessionCount);
        Assert.Equal(100d, item.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenRestrictedToEndDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid trackId, _, Guid bookId, _) = await SeedConsumptionScenarioAsync(userId);
        string toUtc = Uri.EscapeDataString("2026-03-02T23:59:59");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-history/recently-consumed?toUtc={toUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        // The end of the range keeps the book session of February and drops only the third track session of March, which started after it.
        Assert.Equal(new[] { trackId, bookId }, history!.Data.Select(item => item.MediaItemId).ToArray());
        Assert.Equal(1, history.Data[0].CountedSessionCount);
        Assert.Equal(1, history.Data[0].CompletedSessionCount);
        Assert.Equal(55d, history.Data[0].ConsumedDurationInSeconds);
        Assert.Equal(s_trackSecondLastActivityOnUtc, history.Data[0].LastConsumedOnUtc);
        Assert.Equal(500d, history.Data[1].ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenPaginationIsRequested_ShouldReturnRequestedPage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.EBook, "Book Library");
        Guid newestBookId = await SeedBookAsync(libraryId, "Newest Book");
        Guid middleBookId = await SeedBookAsync(libraryId, "Middle Book");
        Guid oldestBookId = await SeedBookAsync(libraryId, "Oldest Book");
        await SeedRecordAsync(userId, libraryId, MediaKind.Book, newestBookId, ConsumptionStatus.Completed, new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc), 60_000, 10);
        await SeedRecordAsync(userId, libraryId, MediaKind.Book, middleBookId, ConsumptionStatus.Completed, new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc), 60_000, 10);
        await SeedRecordAsync(userId, libraryId, MediaKind.Book, oldestBookId, ConsumptionStatus.Completed, new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), 60_000, 10);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/recently-consumed?currentPage=2&perPage=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        Assert.Equal(2, history!.CurrentPage);
        Assert.Equal(2, history.PerPage);
        Assert.Equal(3, history.Count);
        Assert.Equal(2, history.NumberOfPages);
        ConsumedItemResponse item = Assert.Single(history.Data);
        Assert.Equal(oldestBookId, item.MediaItemId);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenAnotherUserHasHistory_ShouldReturnOnlyCurrentUserItems()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid ownLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook, "Own Library");
        Guid ownBookId = await SeedBookAsync(ownLibraryId, "Own Book");
        await SeedRecordAsync(userId, ownLibraryId, MediaKind.Book, ownBookId, ConsumptionStatus.Completed, new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), 60_000, 10);
        Guid otherUserId = await SeedUserAsync($"otheruser_{Guid.NewGuid()}");
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId, LibraryType.EBook, "Other Library");
        Guid otherBookId = await SeedBookAsync(otherLibraryId, "Other User Secret Book");
        await SeedRecordAsync(otherUserId, otherLibraryId, MediaKind.Book, otherBookId, ConsumptionStatus.Completed, new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc), 60_000, 10);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/recently-consumed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(ownBookId, item.MediaItemId);
        Assert.Equal("Own Book", item.Title);
        Assert.DoesNotContain(history.Data, candidate => candidate.Title == "Other User Secret Book");
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenMediaItemNoLongerExists_ShouldReturnCountsWithBlankDisplayData()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        Guid missingTrackId = Guid.NewGuid();
        await SeedRecordAsync(userId, libraryId, MediaKind.MusicTrack, missingTrackId, ConsumptionStatus.Completed, s_trackFirstStartedOnUtc, s_missingItemLastActivityOnUtc, 30_000, 0);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-history/recently-consumed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ConsumedItemResponse>? history = await DeserializeAsync<PaginatedResponse<ConsumedItemResponse>>(response);
        Assert.NotNull(history);
        ConsumedItemResponse item = Assert.Single(history!.Data);
        Assert.Equal(MediaKind.MusicTrack, item.MediaKind);
        Assert.Equal(missingTrackId, item.MediaItemId);
        Assert.Equal(libraryId, item.LibraryId);
        Assert.False(item.IsAvailable);
        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.CollectionTitle);
        Assert.Equal(string.Empty, item.CreatorName);
        Assert.Null(item.CoverPath);
        Assert.Equal(s_missingItemLastActivityOnUtc, item.LastConsumedOnUtc);
        Assert.Equal(1, item.CountedSessionCount);
        Assert.Equal(1, item.CompletedSessionCount);
        Assert.Equal(30d, item.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = "/api/v1/users/me/consumption-history/recently-consumed";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/recently-consumed";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/recently-consumed";
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
    public async Task GetRecentlyConsumed_WhenMediaKindIsNotAMemberName_ShouldReturnValidationProblem(string mediaKind)
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/recently-consumed";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetRecentlyConsumed_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-history/recently-consumed";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
    }

    /// <summary>
    /// Seeds two libraries owned by <paramref name="userId"/>, a track with three sessions, and a book with one session.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the libraries and the consumption records.</param>
    /// <returns>The Ids of the music library, the track, the ebook library, and the book, and the unique name of the book author.</returns>
    private async Task<(Guid musicLibraryId, Guid trackId, Guid bookLibraryId, Guid bookId, string authorName)> SeedConsumptionScenarioAsync(Guid userId)
    {
        // The contributor display name is unique across the whole database, so every test seeds its own spelling of the author.
        string authorName = $"J. R. R. Tolkien {Guid.NewGuid():N}";
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music, "Music Library");
        Guid trackId = await SeedTrackAsync(musicLibraryId, "Bohemian Rhapsody", "A Night at the Opera", "Queen", []);
        Guid bookLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook, "Book Library");
        Guid bookId = await SeedBookAsync(bookLibraryId, "The Fellowship of the Ring", authorName, []);

        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Completed, s_trackFirstStartedOnUtc, s_trackFirstLastActivityOnUtc, 40_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Stopped, s_trackSecondStartedOnUtc, s_trackSecondLastActivityOnUtc, 15_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, trackId, ConsumptionStatus.Stopped, s_trackThirdStartedOnUtc, s_trackThirdLastActivityOnUtc, 45_000, 0);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.Completed, s_bookStartedOnUtc, s_bookLastActivityOnUtc, 500_000, 10);

        return (musicLibraryId, trackId, bookLibraryId, bookId, authorName);
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
    /// Seeds a book of <paramref name="libraryId"/>, together with an author contributor when a name is supplied.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <param name="authorName">The name of the author of the book, or <see langword="null"/> for no contributor.</param>
    /// <param name="tagNames">The tag names carried by the book.</param>
    /// <returns>The Id of the seeded book.</returns>
    private async Task<Guid> SeedBookAsync(Guid libraryId, string bookTitle, string? authorName = null, IReadOnlyList<string>? tagNames = null)
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
        foreach (string tagName in tagNames ?? [])
            book.Tags.Add(_tagEntityFixture.Create(name: tagName));
        if (contributorId is not null)
            book.Contributors.Add(_bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: contributorId.Value, role: MediaContributorRole.Author));
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return bookId;
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
    /// <param name="lastActivityOnUtc">The date and time when the session last received activity.</param>
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
        DateTime lastActivityOnUtc,
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
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: status == ConsumptionStatus.InProgress ? null : lastActivityOnUtc.AddMinutes(1),
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
