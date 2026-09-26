#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;

/// <summary>
/// Contains integration tests for the <see cref="GetTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTrackEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetTrack_WhenTrackBelongsToAnOwnedLibrary_ShouldReturnTrackDetails()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, "Bohemian Rhapsody", "/music/queen/bohemian-rhapsody.flac");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        TrackResponse? track = JsonSerializer.Deserialize<TrackResponse>(content, _jsonOptions);
        Assert.NotNull(track);
        Assert.Equal(trackId, track!.Id);
        Assert.Equal(albumId, track.AlbumId);
        Assert.Equal(libraryId, track.LibraryId);
        Assert.Equal("/music/queen/bohemian-rhapsody.flac", track.Path);
        Assert.Equal("Bohemian Rhapsody", track.Metadata.Title);
    }

    [Fact]
    public async Task GetTrack_WhenTrackDoesNotExist_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId, _) = await SeedLibraryArtistAlbumAndTrackAsync(userId, "Love of My Life", "/music/queen/a-night-at-the-opera/love-of-my-life.flac");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetTrack_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
            dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
            await dbContext.SaveChangesAsync();
        }

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("AlbumNotFound", problemDetails.Detail);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetTrack_WhenTrackRequestedThroughDifferentAlbum_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, _, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, "Love of My Life", "/music/queen/a-night-at-the-opera/love-of-my-life.flac");
        Guid otherAlbumId = await SeedAlbumAsync(libraryId, artistId, "News of the World");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{otherAlbumId}/tracks/{trackId}");

        // Assert
        // The route album id is enforced, so a track of another album is reported as not found, without disclosing that the track exists.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrack_WhenTrackRequestedThroughDifferentArtist_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, "Love of My Life", "/music/queen/a-night-at-the-opera/love-of-my-life.flac");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{albumId}/tracks/{trackId}");

        // Assert
        // The route artist id is enforced, so an album of another artist is reported as not found, without disclosing that it exists.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("AlbumNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrack_WhenTrackRequestedThroughDifferentLibrary_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, "Love of My Life", "/music/queen/a-night-at-the-opera/love-of-my-life.flac");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{artistId}/albums/{albumId}/tracks/{trackId}");

        // Assert
        // The route library id is enforced, so an album of another library is reported as not found, without disclosing that it exists.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("AlbumNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrack_WhenTrackBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherArtistId, Guid otherAlbumId, Guid otherTrackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, "We Are the Champions", "/music/queen/news-of-the-world/we-are-the-champions.flac");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{otherLibraryId}/artists/{otherArtistId}/albums/{otherAlbumId}/tracks/{otherTrackId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using JsonDocument problemDetails = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("General.Unauthorized", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("NotAuthorized", problemDetails.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("Exception", problemDetails.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTrack_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/not-a-guid/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}");

        // Assert
        // The route value is kept as a raw string, so the unparseable Id reaches the query validator, which reports a clean validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("LibraryIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrack_WhenArtistIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/not-a-guid/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("ArtistIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrack_WhenAlbumIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/not-a-guid/tracks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("AlbumIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrack_WhenTrackIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/not-a-guid");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("TrackIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="trackTitle">The title of the seeded track.</param>
    /// <param name="path">The file system path of the seeded track.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId, string trackTitle, string path)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: path, title: trackTitle, includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId, trackId);
    }

    /// <summary>
    /// Seeds an additional album belonging to the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="title">The title of the seeded album.</param>
    /// <returns>The Id of the seeded album.</returns>
    private async Task<Guid> SeedAlbumAsync(Guid libraryId, Guid artistId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid albumId = Guid.NewGuid();
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: title));
        await dbContext.SaveChangesAsync();
        return albumId;
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user and its username.</returns>
    private async Task<(Guid userId, string username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        return (userId, username);
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
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
