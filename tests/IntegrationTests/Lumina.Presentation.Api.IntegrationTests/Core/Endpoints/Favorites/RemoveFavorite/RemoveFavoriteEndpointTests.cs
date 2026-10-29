#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.RemoveFavorite;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Contains integration tests for the <see cref="RemoveFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededFavoriteIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RemoveFavoriteEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task RemoveFavorite_WhenTheFavoriteExists_ShouldDeleteTheCallersRowAndReturnOk()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/me/favorites/MusicTrack/{mediaItemId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task RemoveFavorite_WhenTheFavoriteDoesNotExist_ShouldReturnOkAndChangeNothing()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid otherMediaItemId = Guid.NewGuid();
        Guid seededFavoriteId = await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, otherMediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/me/favorites/MusicTrack/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Favorites.CountAsync(favorite => favorite.UserId == userId));
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.Id == seededFavoriteId));
    }

    [Fact]
    public async Task RemoveFavorite_WhenAnotherUserFavoritedTheSameItem_ShouldRemoveOnlyTheCallersRow()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, mediaItemId);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherFavoriteId = await SeedFavoriteAsync(otherUserId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/me/favorites/MusicTrack/{mediaItemId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId));
        FavoriteEntity otherFavorite = await dbContext.Favorites.AsNoTracking().FirstAsync(favorite => favorite.Id == otherFavoriteId);
        Assert.Equal(otherUserId, otherFavorite.UserId);
        Assert.Equal(mediaItemId, otherFavorite.MediaItemId);
    }

    [Fact]
    public async Task RemoveFavorite_WhenTheMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/me/favorites/{nameof(FavoriteMediaKind.None)}/{mediaItemId}");

        // Assert
        await AssertValidationProblemAsync(response, "FavoriteMediaKindIsInvalid");
        AssertCacheHeaders(response);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task RemoveFavorite_WhenTheMediaKindIsNotAName_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/me/favorites/NotAMediaKind/{mediaItemId}");

        // Assert
        await AssertValidationProblemAsync(response, "FavoriteMediaKindIsInvalid");
        AssertCacheHeaders(response);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task RemoveFavorite_WhenTheMediaItemIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, mediaItemId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/users/me/favorites/MusicTrack/not-a-guid");

        // Assert
        await AssertValidationProblemAsync(response, "FavoriteMediaItemIdCannotBeEmpty");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task RemoveFavorite_WhenBothRouteValuesAreInvalid_ShouldReturnBothValidationProblems()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/me/favorites/NotAMediaKind/{Guid.Empty}");

        // Assert
        await AssertValidationProblemAsync(response, "FavoriteMediaKindIsInvalid", "FavoriteMediaItemIdCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    /// <summary>
    /// Seeds a favorite of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the favorite.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    /// <returns>The Id of the seeded favorite.</returns>
    private async Task<Guid> SeedFavoriteAsync(Guid userId, FavoriteMediaKind mediaKind, Guid mediaItemId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid favoriteId = Guid.NewGuid();
        dbContext.Favorites.Add(_favoriteEntityFixture.Create(
            id: favoriteId,
            userId: userId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            createdBy: userId,
            includeUpdatedOnUtc: false,
            includeUpdatedBy: false));
        await dbContext.SaveChangesAsync();
        _seededFavoriteIds.Add(favoriteId);
        return favoriteId;
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and tracks it for cleanup.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUserIds.Add(userId);
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
        UserEntity user = dbContext.Users.First(candidate => candidate.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Asserts that the response carries the cache headers of a private, authorization varying resource.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertCacheHeaders(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
    }

    /// <summary>
    /// Asserts that the response is a validation problem details document carrying the expected error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedErrorCodes">The validation error codes that must all be present, in order.</param>
    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(422, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Equal(expectedErrorCodes, errors!["General.Validation"]);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            dbContext.Favorites.RemoveRange(dbContext.Favorites.Where(favorite => _seededFavoriteIds.Contains(favorite.Id)));
            dbContext.Users.RemoveRange(dbContext.Users.Where(user => _seededUserIds.Contains(user.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
