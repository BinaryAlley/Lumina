#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Seed;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.StartConsumption;
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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Contains integration tests for the <see cref="StartConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public StartConsumptionEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task StartConsumption_WhenLibraryAndTrackExist_ShouldCreateInProgressRecord()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{libraryId}/media-items/MusicTrack/{trackId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.NotEqual(Guid.Empty, record!.Id);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(trackId, record.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Null(record.EndedOnUtc);
        Assert.Equal(0.0, record.ConsumedDurationInSeconds);
        Assert.Null(record.LastPosition);
        Assert.Equal(0.0, record.LastProgressFraction);
        Assert.Equal(0, record.ConsecutivePagesRead);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(candidate => candidate.Id == record.Id);
        Assert.NotNull(storedRecord);
        Assert.Equal(userId, storedRecord!.UserId);
        Assert.Equal(libraryId, storedRecord.LibraryId);
        Assert.Equal(trackId, storedRecord.MediaItemId);
        Assert.Equal(ConsumptionStatus.InProgress, storedRecord.Status);
    }

    [Fact]
    public async Task StartConsumption_WhenLibraryAndBookExist_ShouldCreateInProgressRecord()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{libraryId}/media-items/Book/{bookId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.NotEqual(Guid.Empty, record!.Id);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(bookId, record.MediaItemId);
        Assert.Equal(MediaKind.Book, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Null(record.EndedOnUtc);
        Assert.Equal(0.0, record.ConsumedDurationInSeconds);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(candidate => candidate.Id == record.Id);
        Assert.NotNull(storedRecord);
        Assert.Equal(userId, storedRecord!.UserId);
        Assert.Equal(MediaKind.Book, storedRecord.MediaKind);
    }

    [Fact]
    public async Task StartConsumption_WhenResumePointExists_ShouldContinueFromStoredPosition()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, shouldTrackConsumptionProgress: true);
        await SeedResumePointAsync(userId, libraryId, bookId, lastPositionLocation: "chapter-4", lastProgressFraction: 0.6);
        string url = $"/api/v1/libraries/{libraryId}/media-items/Book/{bookId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.Equal(libraryId, record!.LibraryId);
        Assert.Equal(bookId, record.MediaItemId);
        Assert.Equal(MediaKind.Book, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Null(record.EndedOnUtc);
        Assert.Equal(0.0, record.ConsumedDurationInSeconds);
        Assert.Equal(0.6, record.LastProgressFraction);
        Assert.NotNull(record.LastPosition);
        Assert.Equal(MediaPositionKind.Location, record.LastPosition!.Kind);
        Assert.Equal("chapter-4", record.LastPosition.Location);
        Assert.Null(record.LastPosition.TimeOffsetSeconds);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaConsumptionRecordEntity? storedRecord = dbContext.MediaConsumptionRecords.FirstOrDefault(candidate => candidate.Id == record.Id);
        Assert.NotNull(storedRecord);
        Assert.Equal(MediaPositionKind.Location, storedRecord!.LastPositionKind);
        Assert.Equal("chapter-4", storedRecord.LastPositionLocation);
        Assert.Equal(0.6, storedRecord.LastProgressFraction);
    }

    [Fact]
    public async Task StartConsumption_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFound()
    {
        // Arrange
        // Only an admin can reach the library lookup for a library that is not owned by the caller, so the admin role is granted to the
        // authenticated test user to exercise the missing library branch instead of the ownership branch that precedes it.
        Guid userId = GetCurrentUserId();
        await GrantAdminRoleAsync(userId);
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "LibraryNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task StartConsumption_WhenTrackDoesNotExist_ShouldReturnTrackNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{libraryId}/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "TrackNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task StartConsumption_WhenBookDoesNotExist_ShouldReturnBookNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryAndBookAsync(userId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{libraryId}/media-items/Book/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "BookNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task StartConsumption_WhenTrackBelongsToAnotherLibrary_ShouldReturnTrackNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: false);
        (_, Guid otherLibraryTrackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{libraryId}/media-items/MusicTrack/{otherLibraryTrackId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", "TrackNotFound", url, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task StartConsumption_WhenLibraryBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherTrackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{otherLibraryId}/media-items/MusicTrack/{otherTrackId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task StartConsumption_WhenMediaKindIsPhoto_ShouldReturnUnsupportedMediaKind()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _) = await SeedLibraryArtistAlbumAndTrackAsync(userId, shouldTrackConsumptionProgress: false);
        string url = $"/api/v1/libraries/{libraryId}/media-items/Photo/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "UnsupportedMediaKind");
    }

    [Fact]
    public async Task StartConsumption_WhenMediaKindIsNumeric_ShouldReturnUnsupportedMediaKind()
    {
        // Arrange
        // The numeric route value parses to MusicTrack as an enumeration value, but only the member names are accepted as a media kind.
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/1/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "UnsupportedMediaKind");
    }

    [Fact]
    public async Task StartConsumption_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/not-a-guid/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "LibraryIdCannotBeEmpty");
    }

    [Fact]
    public async Task StartConsumption_WhenMediaItemIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/MusicTrack/not-a-guid/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertUnprocessableEntityAsync(response, url, "MediaItemIdCannotBeEmpty");
    }

    [Fact]
    public async Task StartConsumption_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsync(url, null);

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
    /// Seeds a resume point of a book for <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the resume point.</param>
    /// <param name="libraryId">The Id of the library that contains the book.</param>
    /// <param name="mediaItemId">The Id of the book to resume.</param>
    /// <param name="lastPositionLocation">The reading location at which the book can be resumed.</param>
    /// <param name="lastProgressFraction">The progress fraction at which the book can be resumed.</param>
    /// <returns>The Id of the seeded resume point.</returns>
    private async Task<Guid> SeedResumePointAsync(Guid userId, Guid libraryId, Guid mediaItemId, string lastPositionLocation, double lastProgressFraction)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid resumePointId = Guid.NewGuid();
        dbContext.MediaResumePoints.Add(_mediaResumePointEntityFixture.Create(
            id: resumePointId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.Location,
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
    /// Grants the admin role to the user identified by <paramref name="userId"/>, so that the library ownership policy allows access to any library.
    /// </summary>
    /// <param name="userId">The Id of the user that receives the admin role.</param>
    private async Task GrantAdminRoleAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        IDataSeedService dataSeedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
        await dataSeedService.SetDefaultAuthorizationRolesAsync(userId, CancellationToken.None);
        await dataSeedService.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);
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
