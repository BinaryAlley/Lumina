#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.RemoveFavorite;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Contains security tests for the <see cref="RemoveFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH = "/api/v1/users/me/favorites";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RemoveFavoriteEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task RemoveFavorite_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH}/MusicTrack/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("1; DELETE FROM Favorites")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task RemoveFavorite_WithInjectionInMediaKind_ShouldRejectWithoutLeakingOrChangingData(string maliciousMediaKind)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack, mediaItemId);
        string url = $"{PATH}/{Uri.EscapeDataString(maliciousMediaKind)}/{mediaItemId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/", "FavoriteMediaKindIsInvalid");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousMediaKind, content, StringComparison.Ordinal);
        await AssertFavoriteExistsAsync(callerId, mediaItemId);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("1; DELETE FROM Favorites")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("Ünïcödé-💥")] // non ascii input
    public async Task RemoveFavorite_WithInjectionInMediaItemId_ShouldRejectWithoutLeakingOrChangingData(string maliciousMediaItemId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack, mediaItemId);
        string url = $"{PATH}/MusicTrack/{Uri.EscapeDataString(maliciousMediaItemId)}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"{PATH}/MusicTrack/", "FavoriteMediaItemIdCannotBeEmpty");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousMediaItemId, content, StringComparison.Ordinal);
        await AssertFavoriteExistsAsync(callerId, mediaItemId);
    }

    [Fact]
    public async Task RemoveFavorite_WhenAnotherUserFavoritedTheSameItem_ShouldRemoveOnlyTheCallersRow()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack, mediaItemId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        await SeedFavoriteAsync(otherUserId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{PATH}/MusicTrack/{mediaItemId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == callerId));
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == otherUserId && favorite.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task RemoveFavorite_WhenSuccessful_ShouldNotLeakSensitiveData()
    {
        // Arrange
        (Guid callerId, string username) = await CreateAuthenticatedUserAsync();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(callerId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{PATH}/MusicTrack/{mediaItemId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);
        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
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
    /// Seeds a favorite of <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the favorite.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    private async Task SeedFavoriteAsync(Guid userId, FavoriteMediaKind mediaKind, Guid mediaItemId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Favorites.Add(_favoriteEntityFixture.Create(
            userId: userId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            createdBy: userId,
            includeUpdatedOnUtc: false,
            includeUpdatedBy: false));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Asserts that the favorite of <paramref name="userId"/> on <paramref name="mediaItemId"/> still exists.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the favorite.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    private async Task AssertFavoriteExistsAsync(Guid userId, Guid mediaItemId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == mediaItemId));
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
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Asserts the shape of a validation problem details response and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstancePrefix">The expected prefix of the request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstancePrefix, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        // The instance is the routed path, which normalizes the escaping of some characters, so only its prefix is asserted.
        Assert.StartsWith(expectedInstancePrefix, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorCode, errors["General.Validation"]);
        AssertNoSensitiveData(content);
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
    /// Removes the test users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
