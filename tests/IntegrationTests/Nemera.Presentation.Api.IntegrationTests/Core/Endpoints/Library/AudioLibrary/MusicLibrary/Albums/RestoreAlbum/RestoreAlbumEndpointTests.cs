#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.RestoreAlbum;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.RestoreAlbum;

/// <summary>
/// Contains integration tests for the <see cref="RestoreAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreAlbumEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedNemeraApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreAlbumEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreAlbumEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task RestoreAlbum_WhenDeletedAlbumBelongsToAnOwnedArtist_ShouldRestoreTheAlbum()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        AlbumEntity? album = await dbContext.Albums.FirstOrDefaultAsync(album => album.Id == albumId);
        Assert.NotNull(album);
        Assert.False(album!.IsDeleted);
        Assert.Null(album.DeletedOnUtc);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumIsNotDeleted_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId, isDeleted: false);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumIsRestoredThroughDifferentLibrary_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid artistId, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{artistId}/albums/{albumId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumIsRestoredThroughDifferentArtist_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, _, Guid albumId) = await SeedLibraryArtistAndAlbumAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{albumId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherArtistId, Guid otherAlbumId) = await SeedLibraryArtistAndAlbumAsync(otherUserId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{otherLibraryId}/artists/{otherArtistId}/albums/{otherAlbumId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAlbum_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAlbum_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAlbum_WhenIdsAreNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/libraries/not-a-guid/artists/not-a-guid/albums/not-a-guid/restore", null);

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
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, an artist of it and an album of the artist, in the requested deletion state.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="isDeleted">Whether the seeded album is soft deleted.</param>
    /// <returns>The Ids of the seeded library, artist and album.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId)> SeedLibraryArtistAndAlbumAsync(Guid userId, bool isDeleted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
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
        album.IsDeleted = isDeleted;
        album.DeletedOnUtc = isDeleted ? DateTime.UtcNow.AddMinutes(-5) : null;
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", albums: [album], includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, album.Id);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
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
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
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
