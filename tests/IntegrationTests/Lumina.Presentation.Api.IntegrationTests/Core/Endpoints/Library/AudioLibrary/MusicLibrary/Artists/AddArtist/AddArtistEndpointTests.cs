#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.AddArtist;
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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.AddArtist;

/// <summary>
/// Contains integration tests for the <see cref="AddArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddArtistEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly AddArtistRequestFixture _addArtistRequestFixture = new();
    private readonly AddAlbumRequestFixture _addAlbumRequestFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddArtistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddArtistEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task AddArtist_WhenCalledWithValidData_ShouldAddArtist()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        AddAlbumRequest album = _addAlbumRequestFixture.Create(
            metadata: _albumMetadataDtoFixture.Create(title: "A Night at the Opera"),
            contributors: [],
            ratings: [],
            tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        ArtistResponse? artistResponse = JsonSerializer.Deserialize<ArtistResponse>(content, _jsonOptions);
        Assert.NotNull(artistResponse);
        Assert.Equal(libraryId, artistResponse!.LibraryId);
        Assert.Equal("Queen", artistResponse.Name);
        Assert.NotNull(artistResponse.Albums);
        AlbumResponse albumResponse = Assert.Single(artistResponse.Albums!);
        Assert.Equal("A Night at the Opera", albumResponse.Metadata.Title);

        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/v1/libraries/{libraryId}/artists/{artistResponse.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task AddArtist_WhenNameIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        AddArtistRequest request = _addArtistRequestFixture.Create(includeName: false, contributors: [], albums: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, $"/api/v1/libraries/{libraryId}/artists", Errors.Music.ArtistNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddArtist_WhenAlbumsAreMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], includeAlbums: false);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, $"/api/v1/libraries/{libraryId}/artists", Errors.Music.AlbumsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddArtist_WhenAlbumMetadataIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        AddAlbumRequest album = _addAlbumRequestFixture.Create(includeMetadata: false, contributors: [], ratings: [], tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, $"/api/v1/libraries/{libraryId}/artists", Errors.Metadata.MetadataCannotBeNull.Description);
    }

    [Fact]
    public async Task AddArtist_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);
        AddAlbumRequest album = _addAlbumRequestFixture.Create(contributors: [], ratings: [], tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{otherLibraryId}/artists", request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", $"/api/v1/libraries/{otherLibraryId}/artists", "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task AddArtist_WhenLibraryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        // only an admin reaches the library lookup of a library that does not exist, because a regular user is rejected by the ownership policy before it
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();
        Guid missingLibraryId = Guid.NewGuid();
        AddAlbumRequest album = _addAlbumRequestFixture.Create(contributors: [], ratings: [], tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);

        // Act
        HttpResponseMessage response = await adminClient.PostAsJsonAsync($"/api/v1/libraries/{missingLibraryId}/artists", request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", Errors.Library.LibraryNotFound.Description, $"/api/v1/libraries/{missingLibraryId}/artists", "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task AddArtist_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        AddAlbumRequest album = _addAlbumRequestFixture.Create(contributors: [], ratings: [], tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsJsonAsync(url, request);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task AddArtist_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddAlbumRequest album = _addAlbumRequestFixture.Create(contributors: [], ratings: [], tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries/not-a-guid/artists", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "/api/v1/libraries/not-a-guid/artists", Errors.Library.LibraryIdCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddArtist_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        AddAlbumRequest album = _addAlbumRequestFixture.Create(contributors: [], ratings: [], tracks: []);
        AddArtistRequest request = _addArtistRequestFixture.Create(name: "Queen", contributors: [], albums: [album]);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/artists", request, cts.Token));
        Assert.Null(exception);
    }

    /// <summary>
    /// Seeds a music library of type music, owned by <paramref name="userId"/>, whose single content location is the temp path.
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
