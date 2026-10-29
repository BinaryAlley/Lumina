#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Favorites;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.AddFavorite;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Contains security tests for the <see cref="AddFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/favorites";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddFavoriteEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task AddFavorite_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        object body = new { mediaKind = "MusicTrack", mediaItemId = Guid.NewGuid() };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, body);

        // Assert
        await AssertUnauthorizedAsync(response, PATH);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("1; DELETE FROM Favorites")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task AddFavorite_WithInjectionInMediaKind_ShouldRejectWithoutLeakingOrChangingData(string maliciousMediaKind)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = maliciousMediaKind, mediaItemId = Guid.NewGuid() });

        // Assert
        string content = await AssertRejectedAsync(response);
        Assert.DoesNotContain(maliciousMediaKind, content, StringComparison.Ordinal);
        await AssertFavoriteCountAsync(callerId, 0);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("1; DELETE FROM Favorites")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task AddFavorite_WithInjectionInMediaItemId_ShouldRejectWithoutLeakingOrChangingData(string maliciousMediaItemId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = maliciousMediaItemId });

        // Assert
        string content = await AssertRejectedAsync(response);
        Assert.DoesNotContain(maliciousMediaItemId, content, StringComparison.Ordinal);
        await AssertFavoriteCountAsync(callerId, 0);
    }

    [Fact]
    public async Task AddFavorite_WhenTheMediaItemBelongsToAnotherUsersLibrary_ShouldReturnTheSameNotFoundAsAMissingItem()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        Guid otherTrackId = await SeedTrackAsync(otherUserId);
        Guid missingTrackId = Guid.NewGuid();

        // Act
        HttpResponseMessage existingItemResponse = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = otherTrackId });
        HttpResponseMessage missingItemResponse = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = missingTrackId });

        // Assert
        // Both answers are indistinguishable, so the route cannot be used as an existence oracle for another user's library.
        Assert.Equal(HttpStatusCode.NotFound, existingItemResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingItemResponse.StatusCode);
        string existingItemContent = await existingItemResponse.Content.ReadAsStringAsync();
        string missingItemContent = await missingItemResponse.Content.ReadAsStringAsync();
        Assert.Equal(GetProblemDetail(existingItemContent), GetProblemDetail(missingItemContent));
        Assert.Equal("TrackNotFound", GetProblemDetail(existingItemContent));
        await AssertFavoriteCountAsync(callerId, 0);
    }

    [Fact]
    public async Task AddFavorite_WhenSuccessful_ShouldNotLeakSensitiveData()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid trackId = await SeedTrackAsync(callerId);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = trackId });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        FavoriteResponse? favorite = JsonSerializer.Deserialize<FavoriteResponse>(content, _jsonOptions);
        Assert.NotNull(favorite);
        Assert.Equal(trackId, favorite!.MediaItemId);
    }

    [Fact]
    public async Task AddFavorite_WhenTheMediaKindIsNotSupported_ShouldRejectWithNonCacheableValidationProblem()
    {
        // Arrange
        (_, string username) = await CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = "Video", mediaItemId = Guid.NewGuid() });

        // Assert
        string content = await AssertValidationProblemAsync(response, PATH, "FavoriteMediaKindIsNotSupported");
        AssertNonCacheable(response);
        Assert.DoesNotContain(username, content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <returns>The Id and username of the created user.</returns>
    private async Task<(Guid UserId, string Username)> CreateAuthenticatedUserAsync()
    {
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id and username.
    /// </summary>
    /// <returns>The Id and username of the seeded user.</returns>
    private async Task<(Guid UserId, string Username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/> with one track, and returns the Id of that track.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the seeded library.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId, userId: userId, title: $"Library-{Guid.NewGuid():N}", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        Guid artistId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Security Artist", includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        Guid albumId = Guid.NewGuid();
        dbContext.Albums.Add(_albumEntityFixture.Create(
            id: albumId,
            artistId: artistId,
            libraryId: libraryId,
            title: "Security Album",
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false));
        await dbContext.SaveChangesAsync();
        Guid trackId = Guid.NewGuid();
        dbContext.Tracks.Add(_trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            title: "Security Track",
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false));
        await dbContext.SaveChangesAsync();
        return trackId;
    }

    /// <summary>
    /// Asserts how many favorites the user identified by <paramref name="userId"/> owns.
    /// </summary>
    /// <param name="userId">The Id of the user whose favorites are counted.</param>
    /// <param name="expectedCount">The expected number of favorites.</param>
    private async Task AssertFavoriteCountAsync(Guid userId, int expectedCount)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(expectedCount, await dbContext.Favorites.CountAsync(favorite => favorite.UserId == userId));
    }

    /// <summary>
    /// Asserts that the response is the unauthenticated problem document produced by the authentication middleware.
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
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Asserts that the request was rejected without succeeding, and returns the raw response body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private static async Task<string> AssertRejectedAsync(HttpResponseMessage response)
    {
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
        AssertNoSensitiveData(content);
        return content;
    }

    /// <summary>
    /// Asserts the shape of a validation problem details response and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorCode, errors["General.Validation"]);
        AssertNoSensitiveData(content);
        return content;
    }

    /// <summary>
    /// Reads the detail of a problem details body.
    /// </summary>
    /// <param name="content">The problem details body to read.</param>
    /// <returns>The problem detail.</returns>
    private static string? GetProblemDetail(string content)
    {
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        return problemDetails!["detail"].GetString();
    }

    /// <summary>
    /// Asserts that the response is not storable by any intermediary and varies by the authorization header.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertNonCacheable(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
    }

    /// <summary>
    /// Asserts that the response body carries no credentials, secrets or stack traces.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoSensitiveData(string content)
    {
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JWT_SECRET, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Data Source", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asserts that the response body exposes only the fields of the documented favorite response contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        Assert.DoesNotContain("version", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedBy", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes the catalogue rows seeded by the test and the test users.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> libraryIds = [.. _seededLibraryIds];
            dbContext.Tracks.RemoveRange(dbContext.Tracks.Where(track => libraryIds.Contains(track.LibraryId)));
            dbContext.Albums.RemoveRange(dbContext.Albums.Where(album => libraryIds.Contains(album.LibraryId)));
            dbContext.Artists.RemoveRange(dbContext.Artists.Where(artist => libraryIds.Contains(artist.LibraryId)));
            dbContext.Libraries.RemoveRange(dbContext.Libraries.Where(library => libraryIds.Contains(library.Id)));
            dbContext.SaveChanges();
        }
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
