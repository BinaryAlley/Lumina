#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetResumePoint;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// Contains integration tests for the <see cref="GetResumePointEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly DateTime s_lastConsumedOnUtc = new(2026, 2, 28, 18, 30, 0, DateTimeKind.Utc);

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
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetResumePointEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetResumePoint_WhenMusicTrackResumePointExists_ShouldReturnResumePoint()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: true);
        Guid resumePointId = await SeedResumePointAsync(userId, libraryId, MediaKind.MusicTrack, trackId, MediaPositionKind.TimeOffset, lastPositionTimeOffsetMilliseconds: 125_500, lastPositionLocation: null, lastProgressFraction: 0.42);
        string url = $"/api/v1/users/me/media-resume-points/MusicTrack/{trackId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ResumePointResponse? resumePoint = JsonSerializer.Deserialize<ResumePointResponse>(content, _jsonOptions);
        Assert.NotNull(resumePoint);
        Assert.Equal(resumePointId, resumePoint!.Id);
        Assert.Equal(libraryId, resumePoint.LibraryId);
        Assert.Equal(trackId, resumePoint.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, resumePoint.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, resumePoint.LastPosition.Kind);
        Assert.Equal(125.5, resumePoint.LastPosition.TimeOffsetSeconds);
        Assert.Null(resumePoint.LastPosition.Location);
        Assert.Equal(0.42, resumePoint.LastProgressFraction);
        Assert.Equal(s_lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
    }

    [Fact]
    public async Task GetResumePoint_WhenBookResumePointExists_ShouldReturnResumePoint()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, shouldTrackConsumptionProgress: true);
        Guid resumePointId = await SeedResumePointAsync(userId, libraryId, MediaKind.Book, bookId, MediaPositionKind.Location, lastPositionTimeOffsetMilliseconds: null, lastPositionLocation: "chapter-12", lastProgressFraction: 0.6);
        string url = $"/api/v1/users/me/media-resume-points/Book/{bookId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ResumePointResponse? resumePoint = JsonSerializer.Deserialize<ResumePointResponse>(content, _jsonOptions);
        Assert.NotNull(resumePoint);
        Assert.Equal(resumePointId, resumePoint!.Id);
        Assert.Equal(libraryId, resumePoint.LibraryId);
        Assert.Equal(bookId, resumePoint.MediaItemId);
        Assert.Equal(MediaKind.Book, resumePoint.MediaKind);
        Assert.Equal(MediaPositionKind.Location, resumePoint.LastPosition.Kind);
        Assert.Null(resumePoint.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-12", resumePoint.LastPosition.Location);
        Assert.Equal(0.6, resumePoint.LastProgressFraction);
        Assert.Equal(s_lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
    }

    [Fact]
    public async Task GetResumePoint_WhenResumePointDoesNotExist_ShouldReturnMediaResumePointNotFound()
    {
        // Arrange
        string url = $"/api/v1/users/me/media-resume-points/MusicTrack/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "MediaResumePointNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task GetResumePoint_WhenResumePointBelongsToAnotherUser_ShouldReturnMediaResumePointNotFoundAndKeepOtherRow()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid mediaItemId = Guid.NewGuid();
        Guid resumePointId = await SeedResumePointAsync(otherUserId, Guid.NewGuid(), MediaKind.MusicTrack, mediaItemId, MediaPositionKind.TimeOffset, lastPositionTimeOffsetMilliseconds: 30_000, lastPositionLocation: null, lastProgressFraction: 0.3);
        string url = $"/api/v1/users/me/media-resume-points/MusicTrack/{mediaItemId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "MediaResumePointNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.NotNull(dbContext.MediaResumePoints.FirstOrDefault(candidate => candidate.Id == resumePointId));
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindIsPhoto_ShouldReturnMediaKindCannotResume()
    {
        // Arrange
        string url = $"/api/v1/users/me/media-resume-points/Photo/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Forbidden", "MediaKindCannotResume", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindIsMovie_ShouldReturnUnsupportedMediaKind()
    {
        // Arrange
        string url = $"/api/v1/users/me/media-resume-points/Movie/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "UnsupportedMediaKind");
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindIsNumeric_ShouldReturnUnsupportedMediaKind()
    {
        // Arrange
        string url = $"/api/v1/users/me/media-resume-points/1/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "UnsupportedMediaKind");
    }

    [Fact]
    public async Task GetResumePoint_WhenMusicTrackNoLongerExists_ShouldReturnTrackNotFoundAndDeleteResumePoint()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: true);
        Guid mediaItemId = Guid.NewGuid();
        Guid resumePointId = await SeedResumePointAsync(userId, libraryId, MediaKind.MusicTrack, mediaItemId, MediaPositionKind.TimeOffset, lastPositionTimeOffsetMilliseconds: 30_000, lastPositionLocation: null, lastProgressFraction: 0.3);
        string url = $"/api/v1/users/me/media-resume-points/MusicTrack/{mediaItemId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "TrackNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");

        // The cleanup is committed before the not-found result is returned, so the deletion survives the request even though the response is not
        // a success response, which the request pipeline would otherwise roll back.
        await AssertResumePointIsDeletedAsync(resumePointId);
    }

    [Fact]
    public async Task GetResumePoint_WhenBookNoLongerExists_ShouldReturnBookNotFoundAndDeleteResumePoint()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryAndBookAsync(userId, shouldTrackConsumptionProgress: true);
        Guid mediaItemId = Guid.NewGuid();
        Guid resumePointId = await SeedResumePointAsync(userId, libraryId, MediaKind.Book, mediaItemId, MediaPositionKind.Location, lastPositionTimeOffsetMilliseconds: null, lastPositionLocation: "chapter-4", lastProgressFraction: 0.4);
        string url = $"/api/v1/users/me/media-resume-points/Book/{mediaItemId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "BookNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");

        // The cleanup is committed before the not-found result is returned, so the deletion survives the request even though the response is not
        // a success response, which the request pipeline would otherwise roll back.
        await AssertResumePointIsDeletedAsync(resumePointId);
    }

    [Fact]
    public async Task GetResumePoint_WhenLibraryNoLongerTracksProgress_ShouldReturnMediaResumePointNotFoundAndDeleteResumePoint()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: false);
        Guid mediaItemId = Guid.NewGuid();
        Guid resumePointId = await SeedResumePointAsync(userId, libraryId, MediaKind.MusicTrack, mediaItemId, MediaPositionKind.TimeOffset, lastPositionTimeOffsetMilliseconds: 30_000, lastPositionLocation: null, lastProgressFraction: 0.3);
        string url = $"/api/v1/users/me/media-resume-points/MusicTrack/{mediaItemId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "MediaResumePointNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");

        // The cleanup is committed before the not-found result is returned, so the deletion survives the request even though the response is not
        // a success response, which the request pipeline would otherwise roll back.
        await AssertResumePointIsDeletedAsync(resumePointId);
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaItemIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = "/api/v1/users/me/media-resume-points/MusicTrack/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "MediaItemIdCannotBeEmpty");
    }

    [Fact]
    public async Task GetResumePoint_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/users/me/media-resume-points/MusicTrack/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="shouldTrackConsumptionProgress">Whether the library tracks the consumption progress of its items.</param>
    /// <returns>The Ids of the seeded library and track.</returns>
    private async Task<(Guid libraryId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId, bool shouldTrackConsumptionProgress)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Music Library", libraryType: LibraryType.Music, contentLocations: [], shouldTrackConsumptionProgress: shouldTrackConsumptionProgress, completionThresholdPercentage: shouldTrackConsumptionProgress ? 100 : null));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: $"/music/queen/{trackId:N}.flac", title: "Bohemian Rhapsody", includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, trackId);
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/> and a book belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="shouldTrackConsumptionProgress">Whether the library tracks the consumption progress of its items.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedLibraryAndBookAsync(Guid userId, bool shouldTrackConsumptionProgress)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Book Library", libraryType: LibraryType.EBook, contentLocations: [], shouldTrackConsumptionProgress: shouldTrackConsumptionProgress, completionThresholdPercentage: shouldTrackConsumptionProgress ? 100 : null));
        dbContext.Books.Add(_bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: "Test Book", includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, bookId);
    }

    /// <summary>
    /// Seeds a resume point of the provided media item for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the resume point.</param>
    /// <param name="libraryId">The Id of the library that contains the media item.</param>
    /// <param name="mediaKind">The kind of media that is resumed.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="lastPositionKind">The kind of the position at which the media item can be resumed.</param>
    /// <param name="lastPositionTimeOffsetMilliseconds">The position at which the media item can be resumed, in milliseconds, when it is a time offset.</param>
    /// <param name="lastPositionLocation">The locator at which the media item can be resumed, when it is not a time offset.</param>
    /// <param name="lastProgressFraction">The progress fraction at which the media item can be resumed.</param>
    /// <returns>The Id of the seeded resume point.</returns>
    private async Task<Guid> SeedResumePointAsync(
        Guid userId,
        Guid libraryId,
        MediaKind mediaKind,
        Guid mediaItemId,
        MediaPositionKind lastPositionKind,
        long? lastPositionTimeOffsetMilliseconds,
        string? lastPositionLocation,
        double lastProgressFraction)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid resumePointId = Guid.NewGuid();
        dbContext.MediaResumePoints.Add(_mediaResumePointEntityFixture.Create(
            id: resumePointId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            lastPositionKind: lastPositionKind,
            lastPositionTimeOffsetMilliseconds: lastPositionTimeOffsetMilliseconds,
            lastPositionLocation: lastPositionLocation,
            lastProgressFraction: lastProgressFraction,
            lastConsumedOnUtc: s_lastConsumedOnUtc));
        await dbContext.SaveChangesAsync();
        return resumePointId;
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
    /// Asserts that the resume point identified by <paramref name="resumePointId"/> is no longer stored once the request transaction has completed.
    /// </summary>
    /// <param name="resumePointId">The Id of the resume point that must be deleted.</param>
    private async Task AssertResumePointIsDeletedAsync(Guid resumePointId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaResumePointEntity? resumePoint = null;
        // The request transaction is committed or rolled back after the response has been sent, so the stored state is read until that completion
        // has run, instead of assuming that it already has.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            resumePoint = dbContext.MediaResumePoints.AsNoTracking().FirstOrDefault(candidate => candidate.Id == resumePointId);
            if (resumePoint is null)
                return;
            await Task.Delay(100);
        }
        Assert.Null(resumePoint);
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
