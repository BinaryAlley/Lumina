#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetDeletedAlbums;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetDeletedAlbums;

/// <summary>
/// Contains integration tests for the <see cref="GetDeletedAlbumsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedAlbumsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedAlbumsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDeletedAlbumsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetDeletedAlbums_WhenLibraryHasDeletedAlbums_ShouldReturnOnlyTheDeletedAlbums()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, name: "Queen");
        AlbumEntity deletedAlbum = await SeedAlbumAsync(libraryId, artistId, title: "A Night at the Opera", isDeleted: true);
        await SeedAlbumAsync(libraryId, artistId, title: "News of the World", isDeleted: false);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/albums/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        DeletedAlbumResponse[]? albums = await response.Content.ReadFromJsonAsync<DeletedAlbumResponse[]>(_jsonOptions);
        Assert.NotNull(albums);
        Assert.Single(albums);
        Assert.Equal(deletedAlbum.Id, albums![0].Id);
        Assert.Equal(libraryId, albums[0].LibraryId);
        Assert.Equal(artistId, albums[0].ArtistId);
        Assert.Equal("Queen", albums[0].ArtistName);
        Assert.Equal("A Night at the Opera", albums[0].Title);
        Assert.NotEqual(default, albums[0].DeletedOnUtc);
    }

    [Fact]
    public async Task GetDeletedAlbums_WhenAlbumBelongsToDeletedArtist_ShouldNotReturnIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        Guid artistId = await SeedArtistAsync(libraryId, name: "Queen");
        await SeedAlbumAsync(libraryId, artistId, title: "A Night at the Opera", isDeleted: true);
        await SoftDeleteArtistAsync(artistId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/albums/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        DeletedAlbumResponse[]? albums = await response.Content.ReadFromJsonAsync<DeletedAlbumResponse[]>(_jsonOptions);
        Assert.NotNull(albums);
        Assert.Empty(albums);
    }

    [Fact]
    public async Task GetDeletedAlbums_WhenLibraryBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherLibraryId = await SeedLibraryAsync(otherUserId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{otherLibraryId}/albums/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedAlbums_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/albums/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedAlbums_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/not-a-guid/albums/deleted-items");

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
    /// Seeds an artist in the provided library.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the artist.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>The Id of the seeded artist.</returns>
    private async Task<Guid> SeedArtistAsync(Guid libraryId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false);
        dbContext.Artists.Add(artist);
        await dbContext.SaveChangesAsync();
        return artist.Id;
    }

    /// <summary>
    /// Seeds an album of the provided artist, in the requested deletion state.
    /// </summary>
    /// <param name="libraryId">The Id of the library that owns the album.</param>
    /// <param name="artistId">The Id of the artist that owns the album.</param>
    /// <param name="title">The title of the album.</param>
    /// <param name="isDeleted">Whether the album is soft deleted.</param>
    /// <returns>The seeded album.</returns>
    private async Task<AlbumEntity> SeedAlbumAsync(Guid libraryId, Guid artistId, string title, bool isDeleted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artistId,
            libraryId: libraryId,
            title: title,
            includeTracks: false,
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        album.IsDeleted = isDeleted;
        album.DeletedOnUtc = isDeleted ? DateTime.UtcNow.AddMinutes(-5) : null;
        dbContext.Albums.Add(album);
        await dbContext.SaveChangesAsync();
        return album;
    }

    /// <summary>
    /// Soft deletes the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist to soft delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SoftDeleteArtistAsync(Guid artistId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        ArtistEntity artist = await dbContext.Artists.FirstAsync(artist => artist.Id == artistId);
        artist.IsDeleted = true;
        artist.DeletedOnUtc = DateTime.UtcNow.AddMinutes(-5);
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
