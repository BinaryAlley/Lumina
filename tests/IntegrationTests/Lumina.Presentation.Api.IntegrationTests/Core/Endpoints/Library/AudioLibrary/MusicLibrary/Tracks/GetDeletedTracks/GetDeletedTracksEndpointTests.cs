#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetDeletedTracks;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetDeletedTracks;

/// <summary>
/// Contains integration tests for the <see cref="GetDeletedTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedTracksEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedTracksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDeletedTracksEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetDeletedTracks_WhenLibraryHasDeletedTracks_ShouldReturnOnlyTheDeletedTracks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);
        TrackEntity deletedTrack = await SeedTrackAsync(libraryId, albumId, title: "Bohemian Rhapsody", path: $"/music/queen/{Guid.NewGuid():N}.flac", isDeleted: true);
        await SeedTrackAsync(libraryId, albumId, title: "Love of My Life", path: $"/music/queen/{Guid.NewGuid():N}.flac", isDeleted: false);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/tracks/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        DeletedTrackResponse[]? tracks = await response.Content.ReadFromJsonAsync<DeletedTrackResponse[]>(_jsonOptions);
        Assert.NotNull(tracks);
        Assert.Single(tracks);
        Assert.Equal(deletedTrack.Id, tracks![0].Id);
        Assert.Equal(libraryId, tracks[0].LibraryId);
        Assert.Equal(artistId, tracks[0].ArtistId);
        Assert.Equal("Queen", tracks[0].ArtistName);
        Assert.Equal(albumId, tracks[0].AlbumId);
        Assert.Equal("A Night at the Opera", tracks[0].AlbumTitle);
        Assert.Equal("Bohemian Rhapsody", tracks[0].Title);
        Assert.Equal(deletedTrack.Path, tracks[0].Path);
        Assert.NotEqual(default, tracks[0].DeletedOnUtc);
    }

    [Fact]
    public async Task GetDeletedTracks_WhenTrackBelongsToDeletedAlbum_ShouldNotReturnIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId);
        await SeedTrackAsync(libraryId, albumId, title: "Bohemian Rhapsody", path: $"/music/queen/{Guid.NewGuid():N}.flac", isDeleted: true);
        await SoftDeleteAlbumAsync(albumId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/tracks/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        DeletedTrackResponse[]? tracks = await response.Content.ReadFromJsonAsync<DeletedTrackResponse[]>(_jsonOptions);
        Assert.NotNull(tracks);
        Assert.Empty(tracks);
    }

    [Fact]
    public async Task GetDeletedTracks_WhenLibraryBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{otherLibraryId}/tracks/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedTracks_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/tracks/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedTracks_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/not-a-guid/tracks/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
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
    /// Seeds a music library owned by <paramref name="userId"/>, an artist of it and an album of the artist.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library, artist and album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistAndAlbumAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artistId,
            libraryId: libraryId,
            title: "A Night at the Opera",
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, album.Id);
    }

    /// <summary>
    /// Seeds a track of the provided album, in the requested deletion state.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the track.</param>
    /// <param name="albumId">The Id of the album that owns the track.</param>
    /// <param name="title">The title of the track.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="isDeleted">Whether the track is soft deleted.</param>
    /// <returns>The seeded track.</returns>
    private async Task<TrackEntity> SeedTrackAsync(Guid libraryId, Guid albumId, string title, string path, bool isDeleted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity track = _trackEntityFixture.Create(
            albumId: albumId,
            libraryId: libraryId,
            path: path,
            title: title,
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        track.IsDeleted = isDeleted;
        track.DeletedOnUtc = isDeleted ? DateTime.UtcNow.AddMinutes(-5) : null;
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        return track;
    }

    /// <summary>
    /// Soft deletes the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="albumId">The Id of the album to soft delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SoftDeleteAlbumAsync(Guid albumId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        AlbumEntity album = await dbContext.Albums.FirstAsync(album => album.Id == albumId);
        album.IsDeleted = true;
        album.DeletedOnUtc = DateTime.UtcNow.AddMinutes(-5);
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
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
