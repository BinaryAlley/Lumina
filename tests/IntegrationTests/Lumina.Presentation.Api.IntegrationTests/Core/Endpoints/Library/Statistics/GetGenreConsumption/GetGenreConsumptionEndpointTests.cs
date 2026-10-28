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
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetGenreConsumption;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// Contains integration tests for the <see cref="GetGenreConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetGenreConsumptionEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
        _seededUsernames.Add(_apiFactory.TestUsername!);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenGenreBearingItemsWereConsumed_ShouldReturnLowercaseGenresOrderedByItemCountThenDurationThenName()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid musicLibraryId, Guid bookLibraryId, string rockGenre, string favouriteGenre, string bookClubGenre, string ignoredGenre) = await SeedGenreConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics/genres");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        List<GenreConsumptionResponse>? genres = await DeserializeAsync<List<GenreConsumptionResponse>>(response);
        Assert.NotNull(genres);
        Assert.Equal(3, genres!.Count);
        Assert.All(genres, genre => Assert.Equal(genre.Name.ToLowerInvariant(), genre.Name));
        Assert.Equal(
            new[] { favouriteGenre.ToLowerInvariant(), rockGenre.ToLowerInvariant(), bookClubGenre.ToLowerInvariant() },
            genres.Select(genre => genre.Name).ToArray());

        Assert.Equal(2, genres[0].ConsumedItemCount);
        Assert.Equal(360d, genres[0].ConsumedDurationInSeconds, 3);
        Assert.Equal(2, genres[1].ConsumedItemCount);
        Assert.Equal(90d, genres[1].ConsumedDurationInSeconds, 3);
        Assert.Equal(1, genres[2].ConsumedItemCount);
        Assert.Equal(300d, genres[2].ConsumedDurationInSeconds, 3);
        Assert.DoesNotContain(genres, genre => genre.Name == ignoredGenre.ToLowerInvariant());
        Assert.NotEqual(musicLibraryId, bookLibraryId);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenRestrictedToOneLibrary_ShouldReturnOnlyThatLibraryGenres()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookLibraryId, _, string favouriteGenre, string bookClubGenre, string ignoredGenre) = await SeedGenreConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/genres?libraryId={bookLibraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<GenreConsumptionResponse>? genres = await DeserializeAsync<List<GenreConsumptionResponse>>(response);
        Assert.NotNull(genres);
        Assert.Equal(
            new[] { bookClubGenre.ToLowerInvariant(), favouriteGenre.ToLowerInvariant() },
            genres!.Select(genre => genre.Name).ToArray());
        Assert.All(genres, genre => Assert.Equal(1, genre.ConsumedItemCount));
        Assert.All(genres, genre => Assert.Equal(300d, genre.ConsumedDurationInSeconds, 3));
        Assert.DoesNotContain(genres, genre => genre.Name == ignoredGenre.ToLowerInvariant());
    }

    [Fact]
    public async Task GetGenreConsumption_WhenRestrictedToOneMediaKind_ShouldReturnOnlyThatKindGenres()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, string rockGenre, string favouriteGenre, string bookClubGenre, _) = await SeedGenreConsumptionScenarioAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/genres?mediaKind={nameof(MediaKind.MusicTrack)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<GenreConsumptionResponse>? genres = await DeserializeAsync<List<GenreConsumptionResponse>>(response);
        Assert.NotNull(genres);
        Assert.Equal(
            new[] { rockGenre.ToLowerInvariant(), favouriteGenre.ToLowerInvariant() },
            genres!.Select(genre => genre.Name).ToArray());
        Assert.Equal(2, genres[0].ConsumedItemCount);
        Assert.Equal(90d, genres[0].ConsumedDurationInSeconds, 3);
        Assert.Equal(1, genres[1].ConsumedItemCount);
        Assert.Equal(60d, genres[1].ConsumedDurationInSeconds, 3);
        Assert.DoesNotContain(genres, genre => genre.Name == bookClubGenre.ToLowerInvariant());
    }

    [Fact]
    public async Task GetGenreConsumption_WhenRestrictedToStartDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, string rockGenre, string favouriteGenre, string bookClubGenre, _) = await SeedGenreConsumptionScenarioAsync(userId);
        string fromUtc = Uri.EscapeDataString("2026-03-01T00:00:00");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/genres?fromUtc={fromUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<GenreConsumptionResponse>? genres = await DeserializeAsync<List<GenreConsumptionResponse>>(response);
        Assert.NotNull(genres);
        Assert.Equal(
            new[] { rockGenre.ToLowerInvariant(), favouriteGenre.ToLowerInvariant() },
            genres!.Select(genre => genre.Name).ToArray());
        Assert.Equal(2, genres[0].ConsumedItemCount);
        Assert.Equal(90d, genres[0].ConsumedDurationInSeconds, 3);
        Assert.Equal(1, genres[1].ConsumedItemCount);
        Assert.Equal(60d, genres[1].ConsumedDurationInSeconds, 3);
        Assert.DoesNotContain(genres, genre => genre.Name == bookClubGenre.ToLowerInvariant());
    }

    [Fact]
    public async Task GetGenreConsumption_WhenRestrictedToEndDate_ShouldReturnOnlySessionsWithinTheRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, _, string rockGenre, string favouriteGenre, string bookClubGenre, _) = await SeedGenreConsumptionScenarioAsync(userId);
        string toUtc = Uri.EscapeDataString("2026-02-28T23:59:59");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/users/me/consumption-statistics/genres?toUtc={toUtc}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<GenreConsumptionResponse>? genres = await DeserializeAsync<List<GenreConsumptionResponse>>(response);
        Assert.NotNull(genres);
        Assert.Equal(
            new[] { bookClubGenre.ToLowerInvariant(), favouriteGenre.ToLowerInvariant() },
            genres!.Select(genre => genre.Name).ToArray());
        Assert.All(genres, genre => Assert.Equal(1, genre.ConsumedItemCount));
        Assert.All(genres, genre => Assert.Equal(300d, genre.ConsumedDurationInSeconds, 3));
        Assert.DoesNotContain(genres, genre => genre.Name == rockGenre.ToLowerInvariant());
    }

    [Fact]
    public async Task GetGenreConsumption_WhenHistoryIsEmpty_ShouldReturnEmptyCollection()
    {
        // Arrange
        _ = GetCurrentUserId();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/consumption-statistics/genres");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<GenreConsumptionResponse>? genres = await DeserializeAsync<List<GenreConsumptionResponse>>(response);
        Assert.NotNull(genres);
        Assert.Empty(genres!);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = "/api/v1/users/me/consumption-statistics/genres";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenLibraryIdIsNotAValidGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/genres";
        string url = $"{path}?libraryId=not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/genres";
        string url = $"{path}?mediaKind={nameof(MediaKind.None)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Theory]
    [InlineData("999")] // a number that is not a member name
    [InlineData("not-a-media-kind")] // a name that is not a member of the enumeration
    public async Task GetGenreConsumption_WhenMediaKindIsNotAMemberName_ShouldReturnValidationProblem(string mediaKind)
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/genres";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "UnsupportedMediaKind");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenDateRangeIsInverted_ShouldReturnValidationProblem()
    {
        // Arrange
        string path = "/api/v1/users/me/consumption-statistics/genres";
        string fromUtc = Uri.EscapeDataString("2026-10-31T00:00:00");
        string toUtc = Uri.EscapeDataString("2026-10-01T00:00:00");
        string url = $"{path}?fromUtc={fromUtc}&toUtc={toUtc}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, path, "ConsumptionDateRangeIsInvalid");
        AssertCacheHeaders(response);
    }

    /// <summary>
    /// Seeds a track and a book in two libraries of <paramref name="userId"/>, with counted and uncounted sessions.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the libraries and the consumption records.</param>
    /// <returns>
    /// The Ids of the music library and the ebook library, and the unique rock, favourite, book club, and ignored genre names that were seeded.
    /// </returns>
    private async Task<(Guid musicLibraryId, Guid bookLibraryId, string rockGenre, string favouriteGenre, string bookClubGenre, string ignoredGenre)> SeedGenreConsumptionScenarioAsync(Guid userId)
    {
        // Each test seeds its own genre rows, so every name carries a unique suffix to keep the shared Genres table collision free.
        string suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        string rockGenre = $"Rock-{suffix}";
        string favouriteGenre = $"Favourite-{suffix}";
        string bookClubGenre = $"Book-Club-{suffix}";
        string ignoredGenre = $"Ignored-{suffix}";

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // The track and the book share a genre, so the genre rows are resolved once per name, and the whole graph is saved through one context.
        Dictionary<string, GenreEntity> genres = [];
        GenreEntity ResolveGenre(string name)
        {
            if (!genres.TryGetValue(name, out GenreEntity? genre))
            {
                genre = _genreEntityFixture.Create(name: name);
                genres[name] = genre;
            }
            return genre;
        }

        Guid musicLibraryId = Guid.NewGuid();
        Guid bookLibraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid uncountedBookId = Guid.NewGuid();

        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: musicLibraryId, userId: userId, title: "Music Library", libraryType: LibraryType.Music, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: bookLibraryId, userId: userId, title: "Book Library", libraryType: LibraryType.EBook, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: musicLibraryId, name: "Queen", includeAlbums: false, includeContributors: false, includeMetadata: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: musicLibraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false));

        TrackEntity firstTrack = _trackEntityFixture.Create(id: firstTrackId, albumId: albumId, libraryId: musicLibraryId, path: $"/music/{firstTrackId:N}.flac", title: "Bohemian Rhapsody", includeMetadata: false);
        firstTrack.Genres.Add(ResolveGenre(rockGenre));
        firstTrack.Genres.Add(ResolveGenre(favouriteGenre));
        dbContext.Tracks.Add(firstTrack);

        TrackEntity secondTrack = _trackEntityFixture.Create(id: secondTrackId, albumId: albumId, libraryId: musicLibraryId, path: $"/music/{secondTrackId:N}.flac", title: "Love of My Life", includeMetadata: false);
        secondTrack.Genres.Add(ResolveGenre(rockGenre));
        dbContext.Tracks.Add(secondTrack);

        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: bookLibraryId, path: $"/books/{bookId:N}.epub", title: "The Fellowship of the Ring", includeMetadata: false);
        book.Genres.Add(ResolveGenre(favouriteGenre));
        book.Genres.Add(ResolveGenre(bookClubGenre));
        dbContext.Books.Add(book);

        BookEntity uncountedBook = _bookEntityFixture.Create(id: uncountedBookId, libraryId: bookLibraryId, path: $"/books/{uncountedBookId:N}.epub", title: "Uncounted Book", includeMetadata: false);
        uncountedBook.Genres.Add(ResolveGenre(ignoredGenre));
        dbContext.Books.Add(uncountedBook);

        await dbContext.SaveChangesAsync();

        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, firstTrackId, ConsumptionStatus.Completed, s_trackCompletedStartedOnUtc, 60_000, 0);
        await SeedRecordAsync(userId, musicLibraryId, MediaKind.MusicTrack, secondTrackId, ConsumptionStatus.Stopped, s_trackStoppedStartedOnUtc, 30_000, 0);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, bookId, ConsumptionStatus.Completed, s_bookCompletedStartedOnUtc, 300_000, 10);
        await SeedRecordAsync(userId, bookLibraryId, MediaKind.Book, uncountedBookId, ConsumptionStatus.Stopped, s_uncountedBookStartedOnUtc, 500_000, 3);

        return (musicLibraryId, bookLibraryId, rockGenre, favouriteGenre, bookClubGenre, ignoredGenre);
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
    /// Removes every user created by the test, together with the libraries it owns.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        foreach (string username in _seededUsernames)
        {
            UserEntity? user = dbContext.Users
                .Include(candidate => candidate.Libraries)
                .FirstOrDefault(candidate => candidate.Username == username);
            if (user is null)
                continue;
            dbContext.Libraries.RemoveRange(user.Libraries);
            dbContext.Users.Remove(user);
        }
        await dbContext.SaveChangesAsync();
    }
}
