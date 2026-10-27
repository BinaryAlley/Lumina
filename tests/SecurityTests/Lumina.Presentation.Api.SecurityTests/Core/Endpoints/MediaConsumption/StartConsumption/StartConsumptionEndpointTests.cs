#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.StartConsumption;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Contains security tests for the <see cref="StartConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string JWT_SECRET = "test-key-thats-at-least-32-chars-long-for-jwt";

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
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public StartConsumptionEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task StartConsumption_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task StartConsumption_WhenSuccessful_ShouldReturnNonCacheableResponseWithoutSensitiveData()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid libraryId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId);
        string url = $"/api/v1/libraries/{libraryId}/media-items/MusicTrack/{trackId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNonCacheable(response);

        string content = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveData(content);
        AssertNoDatabaseOnlyFields(content);

        ConsumptionRecordResponse? record = JsonSerializer.Deserialize<ConsumptionRecordResponse>(content, _jsonOptions);
        Assert.NotNull(record);
        Assert.NotEqual(Guid.Empty, record!.Id);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(trackId, record.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
    }

    [Fact]
    public async Task StartConsumption_WhenMediaItemDoesNotExist_ShouldReturnNonCacheableNotFoundWithoutSensitiveData()
    {
        // Arrange
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        Guid libraryId = await SeedLibraryAsync(userId);
        string url = $"/api/v1/libraries/{libraryId}/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        string content = await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "General.NotFound",
            "TrackNotFound",
            url,
            "https://tools.ietf.org/html/rfc9110#section-15.5.5");
        AssertNonCacheable(response);
        AssertNoSensitiveData(content);
    }

    [Fact]
    public async Task StartConsumption_WhenLibraryBelongsToAnotherUser_ShouldReturnForbiddenAndCreateNoRecord()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherTrackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId);
        (Guid attackerId, _) = await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/libraries/{otherLibraryId}/media-items/MusicTrack/{otherTrackId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        string content = await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Forbidden,
            "General.Unauthorized",
            "NotAuthorized",
            url,
            "https://tools.ietf.org/html/rfc9110#section-15.5.4");
        AssertNonCacheable(response);
        AssertNoSensitiveData(content);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(dbContext.MediaConsumptionRecords.Any(record => record.UserId == attackerId));
        Assert.Equal(otherUserId, dbContext.Libraries.First(library => library.Id == otherLibraryId).UserId);
    }

    [Theory]
    [InlineData("0")] // MediaKind.None, a defined enumeration member
    [InlineData("1")] // MediaKind.MusicTrack, a defined enumeration member
    public async Task StartConsumption_WhenMediaKindIsNumeric_ShouldReturnUnsupportedMediaKindAndCreateNoRecord(string numericMediaKind)
    {
        // Arrange
        // Only the member names are accepted as a media kind, so a numeric route value that parses as a defined enumeration member must be rejected before any record is created.
        (Guid userId, _) = await CreateAuthenticatedUserAsync(_client);
        (Guid libraryId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId);
        string url = $"/api/v1/libraries/{libraryId}/media-items/{numericMediaKind}/{trackId}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        string content = await AssertValidationProblemAsync(response, url, "UnsupportedMediaKind");
        AssertNoSensitiveData(content);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(dbContext.MediaConsumptionRecords.Any(record => record.UserId == userId));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task StartConsumption_WithSqlInjectionInLibraryId_ShouldRejectWithoutLeaking(string maliciousLibraryId)
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/media-items/MusicTrack/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertRejectedInjectionAsync(response, maliciousLibraryId, url, "LibraryIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task StartConsumption_WithSqlInjectionInMediaKind_ShouldRejectWithoutLeaking(string maliciousMediaKind)
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/{Uri.EscapeDataString(maliciousMediaKind)}/{Guid.NewGuid()}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertRejectedInjectionAsync(response, maliciousMediaKind, url, "UnsupportedMediaKind");
    }

    [Theory]
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; DROP TABLE MediaConsumptionRecords--")] // destructive injection
    [InlineData("1; DELETE FROM MediaConsumptionRecords")] // stacked statement injection
    [InlineData("../../../etc/passwd")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task StartConsumption_WithSqlInjectionInMediaItemId_ShouldRejectWithoutLeaking(string maliciousMediaItemId)
    {
        // Arrange
        await CreateAuthenticatedUserAsync(_client);
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/media-items/MusicTrack/{Uri.EscapeDataString(maliciousMediaItemId)}/consumption-records";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        await AssertRejectedInjectionAsync(response, maliciousMediaItemId, url, "MediaItemIdCannotBeEmpty");
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
    /// Seeds a music library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Music Library", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and track.</returns>
    private async Task<(Guid LibraryId, Guid TrackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Music Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: $"/music/queen/{trackId:N}.flac", title: "Bohemian Rhapsody", includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, trackId);
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
    /// Asserts the shape of a problem details response with the provided status, title, detail, instance and type.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="statusCode">The expected HTTP status code.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedType">The expected problem type URI.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertProblemDetailsAsync(HttpResponseMessage response, HttpStatusCode statusCode, string expectedTitle, string expectedDetail, string expectedInstance, string expectedType)
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
        return content;
    }

    /// <summary>
    /// Asserts that the response is the standard validation problem of the start consumption route.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorDescription">The error description expected under the validation errors.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorDescription)
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
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorDescription, errors["General.Validation"]);
        return content;
    }

    /// <summary>
    /// Asserts that a malicious route value is rejected without disclosing internals or reaching the data access layer.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="payload">The malicious value that was sent.</param>
    /// <param name="requestPath">The request path the value was sent in.</param>
    /// <param name="expectedErrorCode">The validation error code expected when the value reaches the validator.</param>
    private async Task AssertRejectedInjectionAsync(HttpResponseMessage response, string payload, string requestPath, string expectedErrorCode)
    {
        string content = await response.Content.ReadAsStringAsync();
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
            Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
            Assert.Equal("General.Validation", problemDetails["title"].GetString());
            // The instance is the request path with the rejected payload still in place, independent of how the server escapes it.
            string escapedPayload = Uri.EscapeDataString(payload);
            int payloadIndex = requestPath.IndexOf(escapedPayload, StringComparison.Ordinal);
            string? instance = problemDetails["instance"].GetString();
            Assert.NotNull(instance);
            Assert.True(payloadIndex >= 0, "The escaped payload must appear in the request path.");
            Assert.StartsWith(requestPath[..payloadIndex], instance, StringComparison.Ordinal);
            Assert.EndsWith(requestPath[(payloadIndex + escapedPayload.Length)..], instance, StringComparison.Ordinal);

            Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
            Assert.NotNull(errors);
            Assert.Contains("General.Validation", errors!.Keys);
            Assert.Contains(expectedErrorCode, errors["General.Validation"]);
        }
        else
        {
            // A payload that carries encoded path separators is rejected by routing before it reaches the validator. That is still a safe rejection, so only it may take this branch.
            Assert.Contains("/", payload, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        AssertNoSensitiveData(content);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the per-user responses are not storable by any intermediary and vary by the authorization header.
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
    /// Asserts that the serialized consumption record exposes only the fields of its documented response contract.
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
    /// Removes the test users created by the test.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
