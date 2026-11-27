#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;

/// <summary>
/// Contains integration tests for the <see cref="UpdateAlbumCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCoverEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumCoverEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateAlbumCoverEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task UpdateAlbumCover_WhenCalledByOwnerWithValidImage_ShouldReplaceTheStoredCover()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumWithCoverAsync(userId, "/media/music/old-cover.jpg");

        // Act
        HttpResponseMessage response = await _client.PutAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover", CreateCoverForm("cover.png"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        string storedPath = JsonSerializer.Deserialize<UpdateAlbumCoverResponse>(content, _jsonOptions)!.CoverPath;
        Assert.False(string.IsNullOrWhiteSpace(storedPath));
        Assert.Contains("cover.png", storedPath, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MusicArtworkEntity coverArtwork = dbContext.MusicArtwork
            .Single(artwork => artwork.OwnerType == MusicArtworkOwnerType.Album && artwork.OwnerId == albumId && artwork.ArtworkType == ArtworkType.Cover);
        Assert.Equal(storedPath, coverArtwork.FileName);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenAlbumHasNoExistingCover_ShouldCreateACoverArtwork()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);

        // Act
        HttpResponseMessage response = await _client.PutAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover", CreateCoverForm("cover.png"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        string storedPath = JsonSerializer.Deserialize<UpdateAlbumCoverResponse>(content, _jsonOptions)!.CoverPath;
        Assert.False(string.IsNullOrWhiteSpace(storedPath));

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MusicArtworkEntity coverArtwork = dbContext.MusicArtwork
            .Single(artwork => artwork.OwnerType == MusicArtworkOwnerType.Album && artwork.OwnerId == albumId && artwork.ArtworkType == ArtworkType.Cover);
        Assert.Equal(storedPath, coverArtwork.FileName);
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, _) = await SeedLibraryArtistAndAlbumAsync(userId);
        string url = $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{Guid.NewGuid()}/cover";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, CreateCoverForm("cover.png"));

        // Assert
        await AssertNotFoundAsync(response, "AlbumNotFound", url);
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);
        Guid otherLibraryId = Guid.NewGuid();
        string url = $"/api/v1/libraries/{otherLibraryId}/artists/{artistId}/albums/{albumId}/cover";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, CreateCoverForm("cover.png"));

        // Assert
        await AssertNotFoundAsync(response, "AlbumNotFound", url);
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenAlbumBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherArtistId, Guid otherAlbumId) = await SeedLibraryArtistAndAlbumAsync(otherUserId);
        string url = $"/api/v1/libraries/{otherLibraryId}/artists/{otherArtistId}/albums/{otherAlbumId}/cover";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, CreateCoverForm("cover.png"));

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/cover";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsync(url, CreateCoverForm("cover.png"));

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenNoFileIsUploaded_ShouldReturnUnprocessableEntityWithCoverCannotBeNull()
    {
        // Arrange
        MultipartFormDataContent form = [];
        form.Add(new StringContent("unrelated-field"), "unrelated-field");
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/cover";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, form);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "CoverCannotBeNull");
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenAlbumIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/{Guid.NewGuid()}/albums/not-a-guid/cover";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, CreateCoverForm("cover.png"));

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "AlbumIdCannotBeEmpty");
    }

    /// <summary>
    /// Creates a multipart form carrying a minimal PNG-like payload.
    /// </summary>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>A configured <see cref="MultipartFormDataContent"/> instance.</returns>
    private static MultipartFormDataContent CreateCoverForm(string fileName)
    {
        // The payload only needs to carry the PNG signature in its first bytes and exceed the header detection buffer.
        byte[] payload = new byte[100];
        payload[0] = 137;
        payload[1] = 80;
        payload[2] = 78;
        payload[3] = 71;
        payload[4] = 13;
        payload[5] = 10;
        payload[6] = 26;
        payload[7] = 10;
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(payload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return form;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist and one of its albums that has no cover yet.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist and album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistAndAlbumAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false, includeBarcode: false, includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId);
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist and one of its albums that already has a cover.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="existingCoverFileName">The relative path of the existing cover of the album.</param>
    /// <returns>The Ids of the seeded library, artist and album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistAndAlbumWithCoverAsync(Guid userId, string existingCoverFileName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false, includeBarcode: false, includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album], includeContributors: false));
        dbContext.MusicArtwork.Add(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 0, fileName: existingCoverFileName, status: ArtworkStatus.Enriched));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId);
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

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
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
