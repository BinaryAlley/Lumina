#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavorites;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Contains integration tests for the <see cref="GetFavoritesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private const string PATH = "/api/v1/users/me/favorites";

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
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededMediaContributorIds = [];
    private readonly List<Guid> _seededFavoriteIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetFavoritesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetFavorites_WhenFavoritesOfSeveralKindsExist_ShouldReturnOnlyTheCallersFavorites()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music);
        Guid artistId = await SeedArtistAsync(musicLibraryId, "Caller Artist");
        Guid albumId = await SeedAlbumAsync(musicLibraryId, artistId, "Caller Album");
        Guid trackId = await SeedTrackAsync(musicLibraryId, albumId, "Caller Track");
        Guid bookLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook);
        Guid bookId = await SeedBookAsync(bookLibraryId, "Caller Book", "Caller Author");
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, trackId);
        await SeedFavoriteAsync(userId, FavoriteMediaKind.Book, bookId);
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherTrackId = await SeedTrackAsync(otherUserId);

        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        Assert.Equal(2, favorites!.Count);
        Assert.Equal(2, favorites.Data.Count);
        Assert.Contains(favorites.Data, favorite => favorite.MediaKind == FavoriteMediaKind.MusicTrack && favorite.MediaItemId == trackId);
        Assert.Contains(favorites.Data, favorite => favorite.MediaKind == FavoriteMediaKind.Book && favorite.MediaItemId == bookId);
        Assert.DoesNotContain(favorites.Data, favorite => favorite.MediaItemId == otherTrackId);
    }

    [Fact]
    public async Task GetFavorites_WhenNoPaginationIsRequested_ShouldReturnEveryFavoriteInOnePageFromTheMostRecent()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid firstFavoriteId = await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, Guid.NewGuid());
        Guid secondFavoriteId = await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, Guid.NewGuid());
        Guid thirdFavoriteId = await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, Guid.NewGuid());
        DateTime now = DateTime.UtcNow;
        // The auditing interceptor stamps the creation time of an added row, so the favoriting order is restored after the insert.
        await SetCreatedOnUtcAsync(firstFavoriteId, now.AddMinutes(-30));
        await SetCreatedOnUtcAsync(secondFavoriteId, now.AddMinutes(-20));
        await SetCreatedOnUtcAsync(thirdFavoriteId, now.AddMinutes(-10));

        // Act
        HttpResponseMessage response = await _client.GetAsync(PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        Assert.Equal(1, favorites!.CurrentPage);
        Assert.Equal(3, favorites.PerPage);
        Assert.Equal(3, favorites.Count);
        Assert.Equal(1, favorites.NumberOfPages);
        FavoriteEntity thirdFavorite = await GetFavoriteAsync(thirdFavoriteId);
        FavoriteEntity secondFavorite = await GetFavoriteAsync(secondFavoriteId);
        FavoriteEntity firstFavorite = await GetFavoriteAsync(firstFavoriteId);
        Assert.Equal(
            new[] { thirdFavorite.MediaItemId, secondFavorite.MediaItemId, firstFavorite.MediaItemId },
            favorites.Data.Select(favorite => favorite.MediaItemId));
    }

    [Fact]
    public async Task GetFavorites_WhenAPageIsRequested_ShouldReturnDisjointRowsAndThePaginationMetadata()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        for (int index = 0; index < 5; index++)
            await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, Guid.NewGuid());
        string firstPageUrl = $"{PATH}?currentPage=1&perPage=2";
        string secondPageUrl = $"{PATH}?currentPage=2&perPage=2";

        // Act
        HttpResponseMessage firstPageResponse = await _client.GetAsync(firstPageUrl);
        HttpResponseMessage secondPageResponse = await _client.GetAsync(secondPageUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);
        PaginatedResponse<FavoriteItemResponse>? firstPage = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(firstPageResponse);
        PaginatedResponse<FavoriteItemResponse>? secondPage = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(secondPageResponse);
        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.Equal(1, firstPage!.CurrentPage);
        Assert.Equal(2, firstPage.PerPage);
        Assert.Equal(5, firstPage.Count);
        Assert.Equal(3, firstPage.NumberOfPages);
        Assert.Equal(2, firstPage.Data.Count);
        Assert.Equal(2, secondPage!.CurrentPage);
        Assert.Equal(2, secondPage.PerPage);
        Assert.Equal(5, secondPage.Count);
        Assert.Equal(3, secondPage.NumberOfPages);
        Assert.Equal(2, secondPage.Data.Count);
        Assert.Empty(firstPage.Data.Select(favorite => favorite.MediaItemId).Intersect(secondPage.Data.Select(favorite => favorite.MediaItemId)));
    }

    [Fact]
    public async Task GetFavorites_WhenAMediaKindIsRequested_ShouldReturnOnlyThatKind()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, trackId);
        await SeedFavoriteAsync(userId, FavoriteMediaKind.Book, bookId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        FavoriteItemResponse favorite = Assert.Single(favorites!.Data);
        Assert.Equal(FavoriteMediaKind.MusicTrack, favorite.MediaKind);
        Assert.Equal(trackId, favorite.MediaItemId);
        Assert.Equal(1, favorites.Count);
    }

    [Fact]
    public async Task GetFavorites_WhenAFavoritedTrackExists_ShouldReturnTheAlbumAsTheCollectionAndTheArtistAsTheCreator()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music);
        Guid artistId = await SeedArtistAsync(musicLibraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(musicLibraryId, artistId, "A Night at the Opera");
        Guid trackId = await SeedTrackAsync(musicLibraryId, albumId, "Bohemian Rhapsody");
        await SeedAlbumCoverAsync(albumId, "a-night-at-the-opera-cover.jpg");
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, trackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        FavoriteItemResponse favorite = Assert.Single(favorites!.Data);
        Assert.True(favorite.IsAvailable);
        Assert.Equal(musicLibraryId, favorite.LibraryId);
        Assert.Equal("Bohemian Rhapsody", favorite.Title);
        Assert.Equal(albumId, favorite.CollectionId);
        Assert.Equal("A Night at the Opera", favorite.CollectionTitle);
        Assert.Equal(artistId, favorite.CreatorId);
        Assert.Equal("Queen", favorite.CreatorName);
        Assert.Equal("a-night-at-the-opera-cover.jpg", favorite.CoverPath);
    }

    [Fact]
    public async Task GetFavorites_WhenAFavoritedBookExists_ShouldReturnTheAuthorAsTheCreatorAndNoCollection()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid bookLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook);
        Guid bookId = await SeedBookAsync(bookLibraryId, "The Fellowship of the Ring", "J. R. R. Tolkien");
        Guid authorId = _seededMediaContributorIds[^1];
        await SeedFavoriteAsync(userId, FavoriteMediaKind.Book, bookId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=Book");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        FavoriteItemResponse favorite = Assert.Single(favorites!.Data);
        Assert.True(favorite.IsAvailable);
        Assert.Equal(bookLibraryId, favorite.LibraryId);
        Assert.Equal("The Fellowship of the Ring", favorite.Title);
        Assert.Equal(Guid.Empty, favorite.CollectionId);
        Assert.Equal(string.Empty, favorite.CollectionTitle);
        Assert.Equal(authorId, favorite.CreatorId);
        Assert.Equal("J. R. R. Tolkien", favorite.CreatorName);
        Assert.Null(favorite.CoverPath);
    }

    [Fact]
    public async Task GetFavorites_WhenTheFavoritedItemWasDeleted_ShouldReturnTheRowAsUnavailableWithEmptyDisplayFields()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid deletedItemId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, deletedItemId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        FavoriteItemResponse favorite = Assert.Single(favorites!.Data);
        Assert.Equal(deletedItemId, favorite.MediaItemId);
        Assert.False(favorite.IsAvailable);
        Assert.Equal(Guid.Empty, favorite.LibraryId);
        Assert.Equal(string.Empty, favorite.Title);
        Assert.Equal(Guid.Empty, favorite.CollectionId);
        Assert.Equal(string.Empty, favorite.CollectionTitle);
        Assert.Equal(Guid.Empty, favorite.CreatorId);
        Assert.Equal(string.Empty, favorite.CreatorName);
        Assert.Null(favorite.CoverPath);
    }

    [Fact]
    public async Task GetFavorites_WhenTheFavoritedItemBelongsToAnInaccessibleLibrary_ShouldStillReturnTheRowAsUnavailable()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherMusicLibraryId = await SeedLibraryAsync(otherUserId, LibraryType.Music);
        Guid otherArtistId = await SeedArtistAsync(otherMusicLibraryId, "Other Artist");
        Guid otherAlbumId = await SeedAlbumAsync(otherMusicLibraryId, otherArtistId, "Other Album");
        Guid otherTrackId = await SeedTrackAsync(otherMusicLibraryId, otherAlbumId, "Other Track");
        Guid userId = GetCurrentUserId();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, otherTrackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=MusicTrack");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<FavoriteItemResponse>? favorites = await DeserializeAsync<PaginatedResponse<FavoriteItemResponse>>(response);
        Assert.NotNull(favorites);
        FavoriteItemResponse favorite = Assert.Single(favorites!.Data);
        Assert.Equal(otherTrackId, favorite.MediaItemId);
        Assert.False(favorite.IsAvailable);
        Assert.Equal(Guid.Empty, favorite.LibraryId);
        Assert.Equal(string.Empty, favorite.Title);
        Assert.Equal(Guid.Empty, favorite.CollectionId);
        Assert.Equal(Guid.Empty, favorite.CreatorId);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Other Track", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Other Artist", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Other Album", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetFavorites_WhenTheMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid trackId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, trackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind={nameof(FavoriteMediaKind.None)}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal(422, problemDetails.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("FavoriteMediaKindIsInvalid", content, StringComparison.Ordinal);
        Assert.DoesNotContain(trackId.ToString(), content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFavorites_WhenTheMediaKindIsNotAMemberName_ShouldNotReturnAnyFavorite()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid trackId = Guid.NewGuid();
        await SeedFavoriteAsync(userId, FavoriteMediaKind.MusicTrack, trackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{PATH}?mediaKind=NotAMediaKind");

        // Assert
        // The request binds the media kind as an enumeration, so an unparseable value is answered by the generic exception handler
        // with the sanitized problem before the handler runs, and no favorite is disclosed.
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(trackId.ToString(), content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a media library of the given type owned by <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the media library.</param>
    /// <param name="libraryType">The type of the media library.</param>
    /// <returns>The Id of the seeded media library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId, LibraryType libraryType)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId, userId: userId, title: $"Library-{Guid.NewGuid():N}", libraryType: libraryType, contentLocations: []));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        return libraryId;
    }

    /// <summary>
    /// Seeds an artist of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>The Id of the seeded artist.</returns>
    private async Task<Guid> SeedArtistAsync(Guid libraryId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid artistId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(
            id: artistId, libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        return artistId;
    }

    /// <summary>
    /// Seeds an album of the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="title">The title of the album.</param>
    /// <returns>The Id of the seeded album.</returns>
    private async Task<Guid> SeedAlbumAsync(Guid libraryId, Guid artistId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(
            id: albumId,
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
        dbContext.Albums.Add(album);
        await dbContext.SaveChangesAsync();
        return albumId;
    }

    /// <summary>
    /// Seeds a track of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library the track belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="title">The title of the track.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, Guid albumId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            title: title,
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();
        return trackId;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/> with one track, and returns the Id of that track.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the seeded library.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid userId)
    {
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music);
        Guid artistId = await SeedArtistAsync(musicLibraryId, "Other Artist");
        Guid albumId = await SeedAlbumAsync(musicLibraryId, artistId, "Other Album");
        return await SeedTrackAsync(musicLibraryId, albumId, "Other Track");
    }

    /// <summary>
    /// Seeds the cover artwork of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="albumId">The Id of the album that owns the artwork.</param>
    /// <param name="fileName">The file name of the stored cover.</param>
    private async Task SeedAlbumCoverAsync(Guid albumId, string fileName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album,
            ownerId: albumId,
            artworkType: ArtworkType.Cover,
            ordinal: 0,
            fileName: fileName));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a book of the library identified by <paramref name="libraryId"/>, with one author.
    /// </summary>
    /// <param name="libraryId">The Id of the library the book belongs to.</param>
    /// <param name="title">The title of the book.</param>
    /// <param name="authorName">The display name of the author of the book.</param>
    /// <returns>The Id of the seeded book.</returns>
    private async Task<Guid> SeedBookAsync(Guid libraryId, string title, string authorName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid bookId = Guid.NewGuid();
        Guid contributorId = Guid.NewGuid();
        dbContext.Books.Add(_bookEntityFixture.Create(id: bookId, libraryId: libraryId, title: title, includeMetadata: false));
        dbContext.MediaContributors.Add(_mediaContributorEntityFixture.Create(id: contributorId, displayName: authorName));
        dbContext.BookContributors.Add(_bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: contributorId, role: MediaContributorRole.Author));
        await dbContext.SaveChangesAsync();
        _seededMediaContributorIds.Add(contributorId);
        return bookId;
    }

    /// <summary>
    /// Seeds a favorite of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the favorite.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    /// <returns>The Id of the seeded favorite.</returns>
    private async Task<Guid> SeedFavoriteAsync(Guid userId, FavoriteMediaKind mediaKind, Guid mediaItemId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid favoriteId = Guid.NewGuid();
        dbContext.Favorites.Add(_favoriteEntityFixture.Create(
            id: favoriteId,
            userId: userId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            createdBy: userId,
            includeUpdatedOnUtc: false,
            includeUpdatedBy: false));
        await dbContext.SaveChangesAsync();
        _seededFavoriteIds.Add(favoriteId);
        return favoriteId;
    }

    /// <summary>
    /// Overrides the creation time of an already stored favorite, so the favoriting order of the test is deterministic.
    /// </summary>
    /// <param name="favoriteId">The Id of the favorite to update.</param>
    /// <param name="createdOnUtc">The creation time to store.</param>
    private async Task SetCreatedOnUtcAsync(Guid favoriteId, DateTime createdOnUtc)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        await dbContext.Favorites
            .Where(favorite => favorite.Id == favoriteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(favorite => favorite.CreatedOnUtc, createdOnUtc));
    }

    /// <summary>
    /// Reads an already stored favorite.
    /// </summary>
    /// <param name="favoriteId">The Id of the favorite to read.</param>
    /// <returns>The stored favorite.</returns>
    private async Task<FavoriteEntity> GetFavoriteAsync(Guid favoriteId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        return await dbContext.Favorites.AsNoTracking().FirstAsync(favorite => favorite.Id == favoriteId);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and tracks it for cleanup.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUserIds.Add(userId);
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
        UserEntity user = dbContext.Users.First(candidate => candidate.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Deserializes the body of <paramref name="response"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the body to.</typeparam>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized body.</returns>
    private async Task<TValue?> DeserializeAsync<TValue>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TValue>(content, _jsonOptions);
    }

    /// <summary>
    /// Asserts that the response carries the cache headers of a private, authorization varying resource.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    private static void AssertCacheHeaders(HttpResponseMessage response)
    {
        Assert.Equal("private, no-store", response.Headers.NonValidated["Cache-Control"].ToString());
        Assert.Equal("Authorization", response.Headers.NonValidated["Vary"].ToString());
    }

    /// <summary>
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> libraryIds = [.. _seededLibraryIds];
            List<Guid> albumIds = await dbContext.Albums.Where(album => libraryIds.Contains(album.LibraryId)).Select(album => album.Id).ToListAsync();
            List<Guid> bookIds = await dbContext.Books.Where(book => libraryIds.Contains(book.LibraryId)).Select(book => book.Id).ToListAsync();
            dbContext.Favorites.RemoveRange(dbContext.Favorites.Where(favorite => _seededFavoriteIds.Contains(favorite.Id)));
            dbContext.MusicArtwork.RemoveRange(dbContext.MusicArtwork.Where(artwork => albumIds.Contains(artwork.OwnerId)));
            dbContext.BookContributors.RemoveRange(dbContext.BookContributors.Where(contributor => bookIds.Contains(contributor.BookId)));
            dbContext.Tracks.RemoveRange(dbContext.Tracks.Where(track => libraryIds.Contains(track.LibraryId)));
            dbContext.Albums.RemoveRange(dbContext.Albums.Where(album => libraryIds.Contains(album.LibraryId)));
            dbContext.Artists.RemoveRange(dbContext.Artists.Where(artist => libraryIds.Contains(artist.LibraryId)));
            dbContext.Books.RemoveRange(dbContext.Books.Where(book => libraryIds.Contains(book.LibraryId)));
            dbContext.Libraries.RemoveRange(dbContext.Libraries.Where(library => libraryIds.Contains(library.Id)));
            dbContext.MediaContributors.RemoveRange(dbContext.MediaContributors.Where(contributor => _seededMediaContributorIds.Contains(contributor.Id)));
            dbContext.Users.RemoveRange(dbContext.Users.Where(user => _seededUserIds.Contains(user.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
