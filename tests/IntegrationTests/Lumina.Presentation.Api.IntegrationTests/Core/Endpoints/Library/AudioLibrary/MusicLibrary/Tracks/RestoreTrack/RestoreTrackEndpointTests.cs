#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;

/// <summary>
/// Contains integration tests for the <see cref="RestoreTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreTrackEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreTrackEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task RestoreTrack_WhenDeletedTrackBelongsToAnOwnedAlbum_ShouldRestoreTheTrack()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity? track = await dbContext.Tracks.FirstOrDefaultAsync(track => track.Id == trackId);
        Assert.NotNull(track);
        Assert.False(track!.IsDeleted);
        Assert.Null(track.DeletedOnUtc);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackIsNotDeleted_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, isDeleted: false);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackIsRestoredThroughDifferentLibrary_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackIsRestoredThroughDifferentAlbum_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, _, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{Guid.NewGuid()}/tracks/{trackId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherArtistId, Guid otherAlbumId, Guid otherTrackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{otherLibraryId}/artists/{otherArtistId}/albums/{otherAlbumId}/tracks/{otherTrackId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTrack_WhenTrackDoesNotExist_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTrack_WhenIdsAreNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/libraries/not-a-guid/artists/not-a-guid/albums/not-a-guid/tracks/not-a-guid/restore", null);

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
    /// Seeds a music library owned by <paramref name="userId"/>, an artist of it, an album of the artist and a track of the album, in the requested deletion state.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="isDeleted">Whether the seeded track is soft deleted.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId, bool isDeleted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            albumId: albumId,
            libraryId: libraryId,
            path: $"/music/queen/{Guid.NewGuid():N}.flac",
            title: "Bohemian Rhapsody",
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        track.IsDeleted = isDeleted;
        track.DeletedOnUtc = isDeleted ? DateTime.UtcNow.AddMinutes(-5) : null;
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artistId,
            libraryId: libraryId,
            title: "A Night at the Opera",
            tracks: [track],
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, album.Id, track.Id);
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
