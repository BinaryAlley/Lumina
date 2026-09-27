#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteArtist;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteArtist;

/// <summary>
/// Contains integration tests for the <see cref="DeleteArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteArtistEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteArtistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteArtistEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task DeleteArtist_WhenArtistBelongsToAnOwnedLibrary_ShouldDeleteTheArtist()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId) = await SeedLibraryAndArtistAsync(userId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Null(await dbContext.Artists.FirstOrDefaultAsync(artist => artist.Id == artistId));
    }

    [Fact]
    public async Task DeleteArtist_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        string url = $"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertNotFoundAsync(response, "ArtistNotFound", url);
    }

    [Fact]
    public async Task DeleteArtist_WhenArtistRequestedThroughDifferentLibrary_ShouldReturnArtistNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid artistId) = await SeedLibraryAndArtistAsync(userId);
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{artistId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertNotFoundAsync(response, "ArtistNotFound", url);
    }

    [Fact]
    public async Task DeleteArtist_WhenArtistBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherArtistId) = await SeedLibraryAndArtistAsync(otherUserId);
        string url = $"/api/v1/libraries/{otherLibraryId}/artists/{otherArtistId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task DeleteArtist_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task DeleteArtist_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/not-a-guid/artists/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "LibraryIdCannotBeEmpty");
    }

    [Fact]
    public async Task DeleteArtist_WhenArtistIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "ArtistIdCannotBeEmpty");
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
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist of that library.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and artist.</returns>
    private async Task<(Guid libraryId, Guid artistId)> SeedLibraryAndArtistAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId);
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
    /// Asserts that the response is a not found problem details carrying the expected detail and request instance path.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertNotFoundAsync(HttpResponseMessage response, string expectedDetail, string expectedInstance)
    {
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", expectedDetail, expectedInstance, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
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
    /// Asserts that the response is an unprocessable entity problem details carrying the expected validation error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCodes">The validation error codes that must all be present.</param>
    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, string expectedInstance, params string[] expectedErrorCodes)
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
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
