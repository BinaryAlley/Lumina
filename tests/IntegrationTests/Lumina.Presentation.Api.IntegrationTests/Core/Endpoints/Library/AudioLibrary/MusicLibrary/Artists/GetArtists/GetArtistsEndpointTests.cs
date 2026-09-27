#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtists;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtists;

/// <summary>
/// Contains integration tests for the <see cref="GetArtistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetArtistsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetArtists_WhenCalledWithoutPaginationData_ShouldReturnAllArtists()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        await SeedArtistAsync(libraryId, "Artist A");
        await SeedArtistAsync(libraryId, "Artist B");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<ArtistResponse>? paginatedArtists = await response.Content.ReadFromJsonAsync<PaginatedResponse<ArtistResponse>>(_jsonOptions);
        Assert.NotNull(paginatedArtists);
        Assert.Equal(2, paginatedArtists!.Count);
        Assert.Equal(2, paginatedArtists.Data.Count);
        Assert.Equal(1, paginatedArtists.CurrentPage);
        Assert.Contains(paginatedArtists.Data, artist => artist.Name == "Artist A");
        Assert.Contains(paginatedArtists.Data, artist => artist.Name == "Artist B");
    }

    [Fact]
    public async Task GetArtists_WhenCalledWithSearchTerm_ShouldReturnOnlyMatchingArtists()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        await SeedArtistAsync(libraryId, "Fellowship");
        await SeedArtistAsync(libraryId, "The Two Towers");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists?searchTerm={Uri.EscapeDataString("Fellowship")}");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<ArtistResponse>? paginatedArtists = await response.Content.ReadFromJsonAsync<PaginatedResponse<ArtistResponse>>(_jsonOptions);
        Assert.NotNull(paginatedArtists);
        ArtistResponse artist = Assert.Single(paginatedArtists!.Data);
        Assert.Equal("Fellowship", artist.Name);
        Assert.Equal(1, paginatedArtists.Count);
    }

    [Fact]
    public async Task GetArtists_WhenCalledWithPaginationData_ShouldReturnRequestedPage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        await SeedArtistAsync(libraryId, "Artist A");
        await SeedArtistAsync(libraryId, "Artist B");
        await SeedArtistAsync(libraryId, "Artist C");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists?currentPage=2&perPage=2");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<ArtistResponse>? paginatedArtists = await response.Content.ReadFromJsonAsync<PaginatedResponse<ArtistResponse>>(_jsonOptions);
        Assert.NotNull(paginatedArtists);
        Assert.Equal(3, paginatedArtists!.Count);
        Assert.Equal(2, paginatedArtists.NumberOfPages);
        Assert.Equal(2, paginatedArtists.CurrentPage);
        Assert.Equal(2, paginatedArtists.PerPage);
        ArtistResponse artist = Assert.Single(paginatedArtists.Data);
        Assert.Equal("Artist C", artist.Name);
    }

    [Fact]
    public async Task GetArtists_WhenNoArtistsExist_ShouldReturnEmptyPaginatedResponse()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<ArtistResponse>? paginatedArtists = await response.Content.ReadFromJsonAsync<PaginatedResponse<ArtistResponse>>(_jsonOptions);
        Assert.NotNull(paginatedArtists);
        Assert.Empty(paginatedArtists!.Data);
        Assert.Equal(0, paginatedArtists.Count);
    }

    [Fact]
    public async Task GetArtists_WhenLibraryBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid libraryId = await SeedLibraryAsync(otherUserId);
        await SeedArtistAsync(libraryId, "Artist A");
        string url = $"/api/v1/libraries/{libraryId}/artists";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task GetArtists_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task GetArtists_WhenRouteIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = "/api/v1/libraries/not-a-guid/artists";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "LibraryIdCannotBeEmpty");
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds an artist belonging to the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedArtistAsync(Guid libraryId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Artists.Add(_artistEntityFixture.Create(libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
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
