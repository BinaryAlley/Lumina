#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Favorites;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.AddFavorite;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Contains integration tests for the <see cref="AddFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededMediaContributorIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddFavoriteEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist)] // a favorite of an artist of a music library
    [InlineData(FavoriteMediaKind.MusicAlbum)] // a favorite of an album of a music library
    [InlineData(FavoriteMediaKind.MusicTrack)] // a favorite of a track of a music library
    [InlineData(FavoriteMediaKind.Book)] // a favorite of a book of a book library
    public async Task AddFavorite_WhenMediaItemExists_ShouldReturnTheFavoriteAndStoreOneRowForTheCaller(FavoriteMediaKind mediaKind)
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Dictionary<FavoriteMediaKind, Guid> mediaItemIds = await SeedCatalogueAsync(userId);
        Guid mediaItemId = mediaItemIds[mediaKind];

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = mediaKind.ToString(), mediaItemId = mediaItemId });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCacheHeaders(response);
        FavoriteResponse? favorite = await DeserializeAsync<FavoriteResponse>(response);
        Assert.NotNull(favorite);
        Assert.NotEqual(Guid.Empty, favorite!.Id);
        Assert.Equal(mediaKind, favorite.MediaKind);
        Assert.Equal(mediaItemId, favorite.MediaItemId);
        Assert.NotEqual(default, favorite.FavoritedOnUtc);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<FavoriteEntity> rows = await dbContext.Favorites
            .AsNoTracking()
            .Where(candidate => candidate.UserId == userId && candidate.MediaKind == mediaKind && candidate.MediaItemId == mediaItemId)
            .ToListAsync();
        FavoriteEntity row = Assert.Single(rows);
        Assert.Equal(favorite.Id, row.Id);
    }

    [Fact]
    public async Task AddFavorite_WhenTheSameItemIsAddedTwice_ShouldReturnTheSameFavoriteAndKeepASingleRow()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = await SeedTrackAsync(userId);

        // Act
        HttpResponseMessage firstResponse = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = mediaItemId });
        HttpResponseMessage secondResponse = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = mediaItemId });

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        FavoriteResponse? firstFavorite = await DeserializeAsync<FavoriteResponse>(firstResponse);
        FavoriteResponse? secondFavorite = await DeserializeAsync<FavoriteResponse>(secondResponse);
        Assert.NotNull(firstFavorite);
        Assert.NotNull(secondFavorite);
        Assert.Equal(firstFavorite!.Id, secondFavorite!.Id);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Favorites.CountAsync(candidate => candidate.UserId == userId && candidate.MediaItemId == mediaItemId));
    }

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist, "ArtistNotFound")] // a missing artist is reported with the artist detail
    [InlineData(FavoriteMediaKind.MusicAlbum, "AlbumNotFound")] // a missing album is reported with the album detail
    [InlineData(FavoriteMediaKind.MusicTrack, "TrackNotFound")] // a missing track is reported with the track detail
    [InlineData(FavoriteMediaKind.Book, "BookNotFound")] // a missing book is reported with the book detail
    public async Task AddFavorite_WhenMediaItemDoesNotExist_ShouldReturnNotFoundWithTheKindSpecificDetail(FavoriteMediaKind mediaKind, string expectedDetail)
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = mediaKind.ToString(), mediaItemId = mediaItemId });

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", expectedDetail, PATH);
        AssertCacheHeaders(response);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(candidate => candidate.UserId == userId && candidate.MediaItemId == mediaItemId));
    }

    [Theory]
    [InlineData(FavoriteMediaKind.Video)] // video libraries are not catalogued yet
    [InlineData(FavoriteMediaKind.Photo)] // photo libraries are not catalogued yet
    public async Task AddFavorite_WhenMediaKindIsPlanned_ShouldReturnValidationProblem(FavoriteMediaKind mediaKind)
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = mediaKind.ToString(), mediaItemId = mediaItemId });

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaKindIsNotSupported");
        AssertCacheHeaders(response);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(candidate => candidate.UserId == userId && candidate.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task AddFavorite_WhenMediaKindIsMissing_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = (string?)null, mediaItemId = mediaItemId });

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaKindIsInvalid");
    }

    [Fact]
    public async Task AddFavorite_WhenMediaKindIsNone_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid mediaItemId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = nameof(FavoriteMediaKind.None), mediaItemId });

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaKindIsInvalid");
        AssertCacheHeaders(response);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(candidate => candidate.UserId == userId && candidate.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task AddFavorite_WhenMediaItemIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = Guid.Empty });

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaItemIdCannotBeEmpty");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(candidate => candidate.UserId == userId && candidate.MediaItemId == Guid.Empty));
    }

    [Fact]
    public async Task AddFavorite_WhenTheBodyIsEmpty_ShouldReturnBothValidationProblems()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { });

        // Assert
        await AssertValidationProblemAsync(response, PATH, "FavoriteMediaKindIsInvalid", "FavoriteMediaItemIdCannotBeEmpty");
    }

    [Fact]
    public async Task AddFavorite_WhenMediaItemBelongsToAnotherUsersLibrary_ShouldReturnTheSameNotFoundAsAMissingItem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherTrackId = await SeedTrackAsync(otherUserId);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, new { mediaKind = "MusicTrack", mediaItemId = otherTrackId });

        // Assert
        // The response is indistinguishable from the one of a missing item, so it cannot be used to probe another user's library.
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "TrackNotFound", PATH);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.False(await dbContext.Favorites.AnyAsync(candidate => candidate.MediaItemId == otherTrackId));
    }

    /// <summary>
    /// Seeds one artist, one album, one track and one book owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the seeded libraries.</param>
    /// <returns>The Ids of the seeded media items, keyed by their kind.</returns>
    private async Task<Dictionary<FavoriteMediaKind, Guid>> SeedCatalogueAsync(Guid userId)
    {
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music);
        Guid artistId = await SeedArtistAsync(musicLibraryId, "Favorite Artist");
        Guid albumId = await SeedAlbumAsync(musicLibraryId, artistId, "Favorite Album");
        Guid trackId = await SeedTrackAsync(musicLibraryId, albumId, "Favorite Track");
        await SeedAlbumCoverAsync(albumId, "favorite-cover.jpg");
        Guid bookLibraryId = await SeedLibraryAsync(userId, LibraryType.EBook);
        Guid bookId = await SeedBookAsync(bookLibraryId, "Favorite Book", "Favorite Author");

        return new Dictionary<FavoriteMediaKind, Guid>
        {
            [FavoriteMediaKind.MusicArtist] = artistId,
            [FavoriteMediaKind.MusicAlbum] = albumId,
            [FavoriteMediaKind.MusicTrack] = trackId,
            [FavoriteMediaKind.Book] = bookId
        };
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/> with one track, and returns the Id of that track.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the seeded library.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid userId)
    {
        Guid musicLibraryId = await SeedLibraryAsync(userId, LibraryType.Music);
        Guid artistId = await SeedArtistAsync(musicLibraryId, "Single Artist");
        Guid albumId = await SeedAlbumAsync(musicLibraryId, artistId, "Single Album");
        return await SeedTrackAsync(musicLibraryId, albumId, "Single Track");
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
    /// Asserts that the response is a problem details document with the expected status, title and detail.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedStatus">The expected HTTP status.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedTitle, string expectedDetail, string expectedInstance)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)expectedStatus, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Asserts that the response is a validation problem details document carrying exactly the expected error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCodes">The validation error codes that must all be present, in order.</param>
    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(422, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Equal(expectedErrorCodes, errors!["General.Validation"]);
        AssertNoSensitiveData(content);
    }

    /// <summary>
    /// Asserts that the response body carries no credentials, secrets or stack traces.
    /// </summary>
    /// <param name="content">The response body to assert on.</param>
    private static void AssertNoSensitiveData(string content)
    {
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
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
