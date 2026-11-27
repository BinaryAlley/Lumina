#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Favorites;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavoriteStatus;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Contains integration tests for the <see cref="GetFavoriteStatusEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private const string PATH = "/api/v1/users/me/favorites/status";

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededFavoriteIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetFavoriteStatusEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetFavoriteStatus_WhenSomeOfTheRequestedItemsAreFavorites_ShouldReturnExactlyThoseIdsOfTheCallerAndKind()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid favoritedTrackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid notFavoritedId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, favoritedTrackId);
        // The book is a favorite of the caller, but of another kind, so it must not be reported for a MusicTrack query.
        await SeedFavoriteAsync(userId, FavoriteMediaKind.Book, bookId);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherUserTrackId = Guid.NewGuid();
        await SeedFavoriteAsync(otherUserId, FavoriteMediaKind.MusicTrack, otherUserTrackId);
        string url = $"{PATH}?mediaKind=MusicTrack&mediaItemIds={favoritedTrackId}&mediaItemIds={bookId}&mediaItemIds={otherUserTrackId}&mediaItemIds={notFavoritedId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        FavoriteStatusResponse? status = await DeserializeAsync<FavoriteStatusResponse>(response);
        Assert.NotNull(status);
        Guid favoritedId = Assert.Single(status!.FavoritedMediaItemIds);
        Assert.Equal(favoritedTrackId, favoritedId);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenTheSameIdIsRequestedTwice_ShouldReturnItOnce()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid favoritedTrackId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, favoritedTrackId);
        string url = $"{PATH}?mediaKind=MusicTrack&mediaItemIds={favoritedTrackId}&mediaItemIds={favoritedTrackId}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        FavoriteStatusResponse? status = await DeserializeAsync<FavoriteStatusResponse>(response);
        Assert.NotNull(status);
        Guid favoritedId = Assert.Single(status!.FavoritedMediaItemIds);
        Assert.Equal(favoritedTrackId, favoritedId);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenOneHundredIdsAreRequested_ShouldAcceptThem()
    {
        // Arrange
        List<Guid> requestedIds = [.. Enumerable.Range(0, 100).Select(_ => Guid.NewGuid())];
        string url = $"{PATH}?mediaKind=MusicTrack&{string.Join("&", requestedIds.Select(requestedId => $"mediaItemIds={requestedId}"))}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        FavoriteStatusResponse? status = await DeserializeAsync<FavoriteStatusResponse>(response);
        Assert.NotNull(status);
        Assert.Empty(status!.FavoritedMediaItemIds);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenMoreThanOneHundredIdsAreRequested_ShouldReturnValidationProblem()
    {
        // Arrange
        List<Guid> requestedIds = [.. Enumerable.Range(0, 101).Select(_ => Guid.NewGuid())];
        string url = $"{PATH}?mediaKind=MusicTrack&{string.Join("&", requestedIds.Select(requestedId => $"mediaItemIds={requestedId}"))}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaItemIdsExceedMaximumCount");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenNoMediaItemIdsAreProvided_ShouldReturnValidationProblem()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=MusicTrack");

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaItemIdsCannotBeEmpty");
        AssertCacheHeaders(response);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenTheEmptyGuidIsRequested_ShouldReturnValidationProblem()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=MusicTrack&mediaItemIds={Guid.Empty}");

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaItemIdCannotBeEmpty");
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenTheMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid favoritedTrackId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, favoritedTrackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind={nameof(FavoriteMediaKind.None)}&mediaItemIds={favoritedTrackId}");

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaKindIsInvalid");
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(favoritedTrackId.ToString(), content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenTheMediaKindIsNotAMemberName_ShouldNotReturnAnyId()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid favoritedTrackId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, favoritedTrackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=NotAMediaKind&mediaItemIds={favoritedTrackId}");

        // Assert
        // The request binds the media kind as an enumeration, so an unparseable value is answered by the generic exception handler
        // with the sanitized problem before the handler runs, and no favorite status is disclosed.
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(favoritedTrackId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
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
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
    }

    /// <summary>
    /// Asserts that the response is a validation problem details document carrying the expected error code.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorCode)
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
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains(expectedErrorCode, errors!["General.Validation"]);
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
