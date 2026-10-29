#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetGenreConsumption;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// Contains security tests for the <see cref="GetUserGenreConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserGenreConsumptionEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";
    private const string PATH_PREFIX = "/api/v1/users/";
    private const string PATH_SUFFIX = "/consumption-statistics/genres";

    private static readonly DateTime s_startedOnUtc = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime s_lastActivityOnUtc = new(2026, 3, 1, 10, 5, 0, DateTimeKind.Utc);

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
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];
    private readonly List<Guid> _adminUserIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserGenreConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetUserGenreConsumptionEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetUserGenreConsumption_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"{PATH_PREFIX}{Guid.NewGuid()}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetUserGenreConsumption_WhenAnotherUsersGenresAreRequested_ShouldReturnForbiddenWithoutDisclosingData()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        string otherGenre = await SeedGenreConsumptionAsync(otherUserId);
        string url = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertForbiddenAsync(response, url);
        AssertNonCacheable(response);
        Assert.DoesNotContain(otherGenre, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("consumedItemCount", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUserGenreConsumption_WhenTargetUserDoesNotExist_ShouldReturnSameForbiddenResultAsForeignUser()
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        await SeedGenreConsumptionAsync(otherUserId);
        string foreignUrl = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";
        string missingUrl = $"{PATH_PREFIX}{Guid.NewGuid()}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage foreignResponse = await _client.GetAsync(foreignUrl);
        HttpResponseMessage missingResponse = await _client.GetAsync(missingUrl);

        // Assert
        await AssertForbiddenAsync(foreignResponse, foreignUrl);
        await AssertForbiddenAsync(missingResponse, missingUrl);
        Dictionary<string, JsonElement> foreignProblem = await ReadProblemDetailsAsync(foreignResponse);
        Dictionary<string, JsonElement> missingProblem = await ReadProblemDetailsAsync(missingResponse);
        Assert.Equal(missingProblem["status"].GetInt32(), foreignProblem["status"].GetInt32());
        Assert.Equal(missingProblem["title"].GetString(), foreignProblem["title"].GetString());
        Assert.Equal(missingProblem["detail"].GetString(), foreignProblem["detail"].GetString());
        Assert.Equal(missingProblem["type"].GetString(), foreignProblem["type"].GetString());
    }

    [Fact]
    public async Task GetUserGenreConsumption_WhenAdminRequestsAnotherUser_ShouldReturnOnlyThatUsersGenres()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        string otherGenre = await SeedGenreConsumptionAsync(otherUserId);
        HttpClient adminClient = _apiFactory.CreateClient();
        Guid adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(adminClient);
        _adminUserIds.Add(adminUserId);
        string url = $"{PATH_PREFIX}{otherUserId}{PATH_SUFFIX}";

        try
        {
            // Act
            HttpResponseMessage response = await adminClient.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertNonCacheable(response);

            string content = await response.Content.ReadAsStringAsync();
            AssertNoSensitiveData(content);
            AssertNoDatabaseOnlyFields(content);

            List<GenreConsumptionResponse>? genres = JsonSerializer.Deserialize<List<GenreConsumptionResponse>>(content, _jsonOptions);
            Assert.NotNull(genres);
            GenreConsumptionResponse genre = Assert.Single(genres!);
            Assert.Equal(otherGenre.ToLowerInvariant(), genre.Name);
            Assert.Equal(1, genre.ConsumedItemCount);
        }
        finally
        {
            adminClient.Dispose();
        }
    }

    [Fact]
    public async Task GetUserGenreConsumption_WhenCallerNamesThemselves_ShouldReturnOwnGenres()
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        string callerGenre = await SeedGenreConsumptionAsync(callerId);
        string url = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        List<GenreConsumptionResponse>? genres = JsonSerializer.Deserialize<List<GenreConsumptionResponse>>(content, _jsonOptions);
        Assert.NotNull(genres);
        GenreConsumptionResponse genre = Assert.Single(genres!);
        Assert.Equal(callerGenre.ToLowerInvariant(), genre.Name);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetUserGenreConsumption_WithInjectionInUserIdRouteValue_ShouldRejectWithoutLeakingData(string maliciousUserId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedGenreConsumptionAsync(callerId);
        string url = $"{PATH_PREFIX}{Uri.EscapeDataString(maliciousUserId)}{PATH_SUFFIX}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertRejectedRouteValueAsync(response, maliciousUserId, PATH_PREFIX, PATH_SUFFIX, "TargetUserIdIsInvalid");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousUserId, content, StringComparison.Ordinal);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.MediaConsumptionRecords.CountAsync(record => record.UserId == callerId));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetUserGenreConsumption_WithInjectionInLibraryId_ShouldRejectWithoutLeakingData(string maliciousLibraryId)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedGenreConsumptionAsync(callerId);
        string path = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";
        string url = $"{path}?libraryId={Uri.EscapeDataString(maliciousLibraryId)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertValidationProblemAsync(response, path, "LibraryIdCannotBeEmpty");
        AssertNonCacheable(response);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.MediaConsumptionRecords.CountAsync(record => record.UserId == callerId));
    }

    [Theory]
    [InlineData("999")] // out of range numeric value
    [InlineData("not-a-media-kind")] // textual value that is not an enum member
    public async Task GetUserGenreConsumption_WithUnsupportedMediaKind_ShouldNotReturnDataOrLeakInternals(string mediaKind)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedGenreConsumptionAsync(callerId);
        string path = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";
        string url = $"{path}?mediaKind={Uri.EscapeDataString(mediaKind)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        // The media kind is carried as text, so an unknown or numeric name reaches the validator and is reported as an
        // unsupported kind instead of being rejected by request binding.
        await AssertRejectedMediaKindAsync(response, path);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetUserGenreConsumption_WithInjectionInFromUtc_ShouldRejectWithoutLeakingData(string maliciousFromUtc)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedGenreConsumptionAsync(callerId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        string otherGenre = await SeedGenreConsumptionAsync(otherUserId);
        string path = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";
        string url = $"{path}?fromUtc={Uri.EscapeDataString(maliciousFromUtc)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertSafeDatePayloadResponseAsync(response, maliciousFromUtc);
        Assert.DoesNotContain(otherGenre, content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.MediaConsumptionRecords.CountAsync(record => record.UserId == otherUserId));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task GetUserGenreConsumption_WithInjectionInToUtc_ShouldRejectWithoutLeakingData(string maliciousToUtc)
    {
        // Arrange
        (Guid callerId, _) = await CreateAuthenticatedUserAsync(_client);
        await SeedGenreConsumptionAsync(callerId);
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        string otherGenre = await SeedGenreConsumptionAsync(otherUserId);
        string path = $"{PATH_PREFIX}{callerId}{PATH_SUFFIX}";
        string url = $"{path}?toUtc={Uri.EscapeDataString(maliciousToUtc)}";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        string content = await AssertSafeDatePayloadResponseAsync(response, maliciousToUtc);
        Assert.DoesNotContain(otherGenre, content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.MediaConsumptionRecords.CountAsync(record => record.UserId == otherUserId));
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate.</param>
    /// <returns>The Id and username of the created user.</returns>
    private async Task<(Guid UserId, string Username)> CreateAuthenticatedUserAsync(HttpClient client)
    {
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
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
        UserEntity user = _userEntityFixture.Create(id: userId, username: username, password: "TestPass123!");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return (userId, username);
    }

    /// <summary>
    /// Seeds a uniquely genred track in a library owned by <paramref name="userId"/> and one completed session of it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the records.</param>
    /// <returns>The unique name of the seeded genre.</returns>
    private async Task<string> SeedGenreConsumptionAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        string suffix = Guid.NewGuid().ToString("N")[..8];
        // Genre names are the primary key of a shared table, so every seeded name carries a unique suffix.
        string genreName = $"Genre-{suffix}";
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: $"Library-{suffix}", libraryType: LibraryType.Music, contentLocations: [], shouldTrackConsumptionProgress: true, completionThresholdPercentage: 100));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: $"Artist-{suffix}", includeAlbums: false, includeContributors: false, includeMetadata: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: $"Album-{suffix}", includeTracks: false, includeMetadata: false));
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: $"/music/{trackId:N}.flac", title: $"Track-{suffix}", includeMetadata: false);
        GenreEntity genre = _genreEntityFixture.Create(name: genreName);
        track.Genres.Add(genre);
        dbContext.Tracks.Add(track);
        dbContext.MediaConsumptionRecords.Add(_mediaConsumptionRecordEntityFixture.Create(
            id: Guid.NewGuid(),
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: s_startedOnUtc,
            lastActivityOnUtc: s_lastActivityOnUtc,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            lastProgressFraction: 1.0,
            consecutivePagesRead: 0));
        await dbContext.SaveChangesAsync();
        return genreName;
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
    /// Asserts that the response is the exact not authorized problem document and returns its raw body.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertForbiddenAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        AssertNoSensitiveData(content);
        return content;
    }

    /// <summary>
    /// Reads the problem details document of <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The parsed problem details fields.</returns>
    private async Task<Dictionary<string, JsonElement>> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        return problemDetails!;
    }

    /// <summary>
    /// Asserts that a malicious route value is rejected without disclosing internals.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="payload">The malicious value that was sent.</param>
    /// <param name="pathPrefix">The request path that precedes the malicious value.</param>
    /// <param name="pathSuffix">The request path that follows the malicious value.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertRejectedRouteValueAsync(HttpResponseMessage response, string payload, string pathPrefix, string pathSuffix, string expectedErrorCode)
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
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        // The instance is the request path with the rejected payload in place, so decoding the middle segment must give the payload back.
        string? instance = problemDetails["instance"].GetString();
        Assert.NotNull(instance);
        Assert.StartsWith(pathPrefix, instance, StringComparison.Ordinal);
        Assert.EndsWith(pathSuffix, instance, StringComparison.Ordinal);
        string encodedSegment = instance![pathPrefix.Length..(instance.Length - pathSuffix.Length)];
        Assert.Equal(payload, Uri.UnescapeDataString(encodedSegment));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorCode, errors["General.Validation"]);
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
    /// Asserts that an unparseable media kind is rejected without returning data or disclosing internals.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertRejectedMediaKindAsync(HttpResponseMessage response, string expectedInstance)
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
        Assert.Contains("UnsupportedMediaKind", errors["General.Validation"]);

        AssertNonCacheable(response);
        AssertNoSensitiveData(content);
        Assert.DoesNotContain("\"consumedItemCount\"", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that a malicious date bound never causes a server error, never leaks internals, and never returns data of another user.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="payload">The malicious value that was sent.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private static async Task<string> AssertSafeDatePayloadResponseAsync(HttpResponseMessage response, string payload)
    {
        string content = await response.Content.ReadAsStringAsync();

        // A bound date is parsed before the handler runs, so an unparseable value must either be ignored or rejected, but it must never reach the
        // storage medium, fail the request, or disclose internals.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Unexpected status code {response.StatusCode} for a date bound payload.");

        AssertNoSensitiveData(content);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
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
    /// Asserts that the response body carries no credentials, secrets, persistence details or stack traces.
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
    /// Asserts that the serialized genres expose only the fields of their documented response contract.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoDatabaseOnlyFields(string content)
    {
        Assert.DoesNotContain("version", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userId", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedBy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("createdOnUtc", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("updatedOnUtc", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes the test users and admin users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
        foreach (Guid userId in _adminUserIds)
            _apiFactory.RemoveAdminUserAsync(userId).GetAwaiter().GetResult();
    }
}
