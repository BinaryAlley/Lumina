#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Contains integration tests for the <see cref="ShuffleTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededUserIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ShuffleTracksEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ShuffleTracks_WhenLibraryHasTracks_ShouldReturnTheTracksWithAllTheirFields()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Queen Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        Guid trackId = await SeedTrackAsync(libraryId, albumId, "Bohemian Rhapsody", 355);
        Guid otherTrackId = await SeedTrackAsync(libraryId, albumId, "Love of My Life", 219);
        await SeedAlbumCoverAsync(albumId, "a-night-at-the-opera-cover.jpg");
        // A second library proves that the response is scoped to the library in the route.
        Guid otherLibraryId = await SeedLibraryAsync(userId, "Other Library");
        Guid otherArtistId = await SeedArtistAsync(otherLibraryId, "ABBA");
        Guid otherAlbumId = await SeedAlbumAsync(otherLibraryId, otherArtistId, "Arrival");
        Guid otherLibraryTrackId = await SeedTrackAsync(otherLibraryId, otherAlbumId, "Dancing Queen", 231);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, 10);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShuffleTrackResponse>? tracks = JsonSerializer.Deserialize<List<ShuffleTrackResponse>>(content, _jsonOptions);
        Assert.NotNull(tracks);
        Assert.Equal(2, tracks!.Count);
        Assert.All(tracks, track => Assert.Contains(track.TrackId, new[] { trackId, otherTrackId }));
        Assert.DoesNotContain(tracks, track => track.TrackId == otherLibraryTrackId);
        ShuffleTrackResponse foundTrack = Assert.Single(tracks, track => track.TrackId == trackId);
        Assert.Equal("Bohemian Rhapsody", foundTrack.Title);
        Assert.Equal(355, foundTrack.DurationInSeconds);
        Assert.Equal(albumId, foundTrack.AlbumId);
        Assert.Equal("A Night at the Opera", foundTrack.AlbumTitle);
        Assert.Equal(artistId, foundTrack.ArtistId);
        Assert.Equal("a-night-at-the-opera-cover.jpg", foundTrack.CoverPath);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShuffleTracks_WhenArtistIdIsProvided_ShouldReturnOnlyTracksOfAlbumsOfThatArtist()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Two Artists Library");
        Guid firstArtistId = await SeedArtistAsync(libraryId, "Queen");
        Guid firstAlbumId = await SeedAlbumAsync(libraryId, firstArtistId, "A Night at the Opera");
        await SeedTrackAsync(libraryId, firstAlbumId, "Bohemian Rhapsody", 355);
        await SeedTrackAsync(libraryId, firstAlbumId, "Love of My Life", 219);
        Guid secondArtistId = await SeedArtistAsync(libraryId, "David Bowie");
        Guid secondAlbumId = await SeedAlbumAsync(libraryId, secondArtistId, "Heroes");
        await SeedTrackAsync(libraryId, secondAlbumId, "Heroes", 371);
        await SeedTrackAsync(libraryId, secondAlbumId, "Beauty and the Beast", 212);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, 10, artistId: firstArtistId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShuffleTrackResponse>? tracks = JsonSerializer.Deserialize<List<ShuffleTrackResponse>>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(tracks);
        Assert.Equal(2, tracks!.Count);
        Assert.All(tracks, track => Assert.Equal(firstArtistId, track.ArtistId));
        Assert.All(tracks, track => Assert.Equal(firstAlbumId, track.AlbumId));
    }

    [Fact]
    public async Task ShuffleTracks_WhenAlbumIdIsProvided_ShouldReturnOnlyTracksOfThatAlbum()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Two Albums Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        await SeedTrackAsync(libraryId, albumId, "Bohemian Rhapsody", 355);
        await SeedTrackAsync(libraryId, albumId, "Love of My Life", 219);
        Guid otherAlbumId = await SeedAlbumAsync(libraryId, artistId, "News of the World");
        await SeedTrackAsync(libraryId, otherAlbumId, "We Are the Champions", 179);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, 10, albumId: albumId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShuffleTrackResponse>? tracks = JsonSerializer.Deserialize<List<ShuffleTrackResponse>>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(tracks);
        Assert.Equal(2, tracks!.Count);
        Assert.All(tracks, track => Assert.Equal(albumId, track.AlbumId));
    }

    [Fact]
    public async Task ShuffleTracks_WhenExcludedTrackIdsAreProvided_ShouldNeverReturnThem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Exclusion Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        Guid firstTrackId = await SeedTrackAsync(libraryId, albumId, "Bohemian Rhapsody", 355);
        Guid secondTrackId = await SeedTrackAsync(libraryId, albumId, "Love of My Life", 219);
        Guid thirdTrackId = await SeedTrackAsync(libraryId, albumId, "The Prophet Song", 367);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(
            libraryId, 10, excludedTrackIds: [firstTrackId, secondTrackId]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShuffleTrackResponse>? tracks = JsonSerializer.Deserialize<List<ShuffleTrackResponse>>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(tracks);
        ShuffleTrackResponse track = Assert.Single(tracks!);
        Assert.Equal(thirdTrackId, track.TrackId);
    }

    [Fact]
    public async Task ShuffleTracks_WhenCountIsLowerThanTheAvailableTracks_ShouldReturnAtMostCountTracks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Count Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        for (int index = 0; index < 5; index++)
            await SeedTrackAsync(libraryId, albumId, $"Track {index}", 200 + index);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, 2);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShuffleTrackResponse>? tracks = JsonSerializer.Deserialize<List<ShuffleTrackResponse>>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(tracks);
        Assert.Equal(2, tracks!.Count);
        Assert.Equal(tracks.Count, tracks.Select(track => track.TrackId).Distinct().Count());
    }

    [Fact]
    public async Task ShuffleTracks_WhenCountIsLargerThanTheAvailableTracks_ShouldReturnEveryAvailableTrack()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Small Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        Guid firstTrackId = await SeedTrackAsync(libraryId, albumId, "Bohemian Rhapsody", 355);
        Guid secondTrackId = await SeedTrackAsync(libraryId, albumId, "Love of My Life", 219);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, 50);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShuffleTrackResponse>? tracks = JsonSerializer.Deserialize<List<ShuffleTrackResponse>>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(tracks);
        Assert.Equal(2, tracks!.Count);
        Assert.Contains(tracks, track => track.TrackId == firstTrackId);
        Assert.Contains(tracks, track => track.TrackId == secondTrackId);
    }

    [Theory]
    [InlineData(0, "ShuffleTrackCountMustBeGreaterThanZero")] // no track is requested
    [InlineData(-1, "ShuffleTrackCountMustBeGreaterThanZero")] // a negative selection size
    [InlineData(501, "ShuffleTrackCountMustBeAtMost500")] // one more than the documented maximum
    public async Task ShuffleTracks_WhenCountIsInvalid_ShouldReturnUnprocessableEntityWithTheValidationError(int count, string expectedError)
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId, "Validation Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        await SeedTrackAsync(libraryId, albumId, "Bohemian Rhapsody", 355);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, count);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.Validation", problemDetails!["title"].GetString());
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails["status"].GetInt32());
        JsonElement errors = problemDetails["errors"];
        Assert.True(errors.TryGetProperty("General.Validation", out JsonElement validationErrors));
        Assert.Contains(validationErrors.EnumerateArray(), error => error.GetString() == expectedError);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShuffleTracks_WhenLibraryBelongsToAnotherUser_ShouldReturnForbiddenWithoutTrackData()
    {
        // Arrange
        (Guid otherUserId, string otherUsername) = await SeedOtherUserAsync();
        Guid libraryId = await SeedLibraryAsync(otherUserId, "Other User Library");
        Guid artistId = await SeedArtistAsync(libraryId, "Queen");
        Guid albumId = await SeedAlbumAsync(libraryId, artistId, "A Night at the Opera");
        await SeedTrackAsync(libraryId, albumId, "Secret Song", 300);

        // Act
        HttpResponseMessage response = await PostShuffleAsync(libraryId, 10);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.Unauthorized", problemDetails!["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Secret Song", content, StringComparison.Ordinal);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(otherUsername));
    }

    /// <summary>
    /// Posts a shuffle request for the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose tracks are shuffled.</param>
    /// <param name="count">The maximum number of tracks to return.</param>
    /// <param name="artistId">Optional. The Id of the artist whose tracks are sampled.</param>
    /// <param name="albumId">Optional. The Id of the album whose tracks are sampled.</param>
    /// <param name="excludedTrackIds">Optional. The tracks that must not be returned.</param>
    /// <returns>The response of the API.</returns>
    private async Task<HttpResponseMessage> PostShuffleAsync(
        Guid libraryId,
        int count,
        Guid? artistId = null,
        Guid? albumId = null,
        IReadOnlyCollection<Guid>? excludedTrackIds = null)
    {
        return await _client.PostAsJsonAsync(
            $"/api/v1/libraries/{libraryId}/tracks/shuffle",
            new { count, artistId, albumId, excludedTrackIds });
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="title">The title of the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId, userId: userId, title: title, libraryType: LibraryType.Music, contentLocations: []));
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
    /// <param name="durationInSeconds">The duration of the track in seconds.</param>
    /// <returns>The Id of the seeded track.</returns>
    private async Task<Guid> SeedTrackAsync(Guid libraryId, Guid albumId, string title, int durationInSeconds)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            path: $"/music/shuffle-{Guid.NewGuid():N}.flac",
            title: title,
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        track.DurationInSeconds = durationInSeconds;
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
            fileName: fileName,
            status: ArtworkStatus.Enriched));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user.
    /// </summary>
    /// <returns>The Id and the username of the seeded user.</returns>
    private async Task<(Guid userId, string username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUserIds.Add(userId);
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
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            foreach (Guid libraryId in _seededLibraryIds)
            {
                List<Guid> albumIds = await dbContext.Albums
                    .Where(album => album.LibraryId == libraryId)
                    .Select(album => album.Id)
                    .ToListAsync();
                dbContext.MusicArtwork.RemoveRange(dbContext.MusicArtwork.Where(artwork => albumIds.Contains(artwork.OwnerId)));
                dbContext.Tracks.RemoveRange(dbContext.Tracks.Where(track => track.LibraryId == libraryId));
                dbContext.Albums.RemoveRange(dbContext.Albums.Where(album => album.LibraryId == libraryId));
                dbContext.Artists.RemoveRange(dbContext.Artists.Where(artist => artist.LibraryId == libraryId));
                dbContext.Libraries.RemoveRange(dbContext.Libraries.Where(library => library.Id == libraryId));
            }
            dbContext.Users.RemoveRange(dbContext.Users.Where(user => _seededUserIds.Contains(user.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
