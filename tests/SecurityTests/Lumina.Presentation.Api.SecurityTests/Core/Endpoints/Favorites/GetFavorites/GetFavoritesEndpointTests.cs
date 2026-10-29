#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavorites;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Contains security tests for the <see cref="GetFavoritesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
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
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetFavoritesEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetFavorites_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

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
    public async Task GetFavorites_WithInjectionInMediaKind_ShouldRejectWithoutLeakingOrChangingData(string maliciousMediaKind)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid favoritedItemId = await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack);
        string url = $"{PATH}?mediaKind={Uri.EscapeDataString(maliciousMediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertRejectedAsync(response);
        Assert.DoesNotContain(maliciousMediaKind, content, StringComparison.Ordinal);
        Assert.DoesNotContain(favoritedItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        await AssertFavoriteCountAsync(callerId, 1);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("1; DELETE FROM Favorites")] // stacked statement injection
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task GetFavorites_WithInjectionInCurrentPage_ShouldRejectWithoutLeakingOrChangingData(string maliciousCurrentPage)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid favoritedItemId = await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack);
        string url = $"{PATH}?currentPage={Uri.EscapeDataString(maliciousCurrentPage)}&perPage=10";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertRejectedAsync(response);
        Assert.DoesNotContain(maliciousCurrentPage, content, StringComparison.Ordinal);
        Assert.DoesNotContain(favoritedItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        await AssertFavoriteCountAsync(callerId, 1);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("1; DELETE FROM Favorites")] // stacked statement injection
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task GetFavorites_WithInjectionInPerPage_ShouldRejectWithoutLeakingOrChangingData(string maliciousPerPage)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid favoritedItemId = await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack);
        string url = $"{PATH}?currentPage=1&perPage={Uri.EscapeDataString(maliciousPerPage)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertRejectedAsync(response);
        Assert.DoesNotContain(maliciousPerPage, content, StringComparison.Ordinal);
        Assert.DoesNotContain(favoritedItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        await AssertFavoriteCountAsync(callerId, 1);
    }

    [Fact]
    public async Task GetFavorites_WhenSuccessful_ShouldReturnOnlyTheCallersFavoritesWithoutSensitiveData()
    {
        // Arrange
        (Guid callerId, string username) = await CreateAuthenticatedUserAsync();
        Guid favoritedItemId = await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack);
        (Guid otherUserId, string otherUsername) = await SeedOtherUserAsync();
        Guid otherFavoritedItemId = await SeedFavoriteAsync(otherUserId, FavoriteMediaKind.MusicTrack);

        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);
        Assert.DoesNotContain(username, content, StringComparison.Ordinal);
        Assert.DoesNotContain(otherUsername, content, StringComparison.Ordinal);
        Assert.Contains(favoritedItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(otherFavoritedItemId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(otherUserId.ToString(), content, StringComparison.OrdinalIgnoreCase);

        PaginatedResponse<FavoriteItemResponse>? favorites = JsonSerializer.Deserialize<PaginatedResponse<FavoriteItemResponse>>(content, _jsonOptions);
        Assert.NotNull(favorites);
        FavoriteItemResponse favorite = Assert.Single(favorites!.Data);
        Assert.Equal(favoritedItemId, favorite.MediaItemId);
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
    /// Seeds a favorite of <paramref name="userId"/> and returns the Id of the favorited media item.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the favorite.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <returns>The Id of the favorited media item.</returns>
    private async Task<Guid> SeedFavoriteAsync(Guid userId, FavoriteMediaKind mediaKind)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid mediaItemId = Guid.NewGuid();
        dbContext.Favorites.Add(_favoriteEntityFixture.Create(
            userId: userId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            createdBy: userId,
            includeUpdatedOnUtc: false,
            includeUpdatedBy: false));
        await dbContext.SaveChangesAsync();
        return mediaItemId;
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
        Assert.DoesNotContain("\"favoritedMediaItemIds\"", content, StringComparison.Ordinal);
        return content;
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
    /// Asserts that the response body exposes only the fields of the documented favorites response contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        Assert.DoesNotContain("version", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdOnUtc", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedOnUtc", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes the test users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
