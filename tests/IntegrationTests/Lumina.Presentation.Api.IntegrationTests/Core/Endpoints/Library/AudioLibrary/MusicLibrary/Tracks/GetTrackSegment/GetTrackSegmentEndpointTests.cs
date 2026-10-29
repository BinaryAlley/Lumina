#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;

/// <summary>
/// Contains integration tests for the <see cref="GetTrackSegmentEndpoint"/> class.
/// </summary>
/// <remarks>
/// The successful production path of a segment is not exercised here, because it requires ffmpeg, which is not part of the test environment. Every
/// covered case is rejected before the normalization pipeline is entered, so these tests are deterministic and environment independent.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly string _contentRootPath = Path.Combine(Path.GetTempPath(), $"lumina-track-segment-tests-{Guid.NewGuid():N}");
    private readonly List<string> _temporaryFiles = [];
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededArtistIds = [];
    private readonly List<Guid> _seededAlbumIds = [];
    private readonly List<Guid> _seededTrackIds = [];
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTrackSegmentEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client and the temporary content root of the test.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        Directory.CreateDirectory(_contentRootPath);
    }

    [Fact]
    public async Task GetTrackSegment_WhenUnauthorized_ShouldReturnUnauthorizedResultWithoutLeakingTheDeliveryHandle()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string trackPath = WriteContentFile("bohemian-rhapsody.ogg", CreateAudioBytes());
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        string deliveryId = await GetDeliveryIdAsync(libraryId, artistId, albumId, trackId);
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(BuildSegmentPath(libraryId, artistId, albumId, trackId, deliveryId, 0));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        // The request path is echoed by the problem details, so the handle the caller sent appears there; no other handle may be disclosed.
        string[] disclosedHandleCandidates = [.. System.Text.RegularExpressions.Regex.Matches(content, "[0-9a-f]{32}").Select(match => match.Value)];
        Assert.All(disclosedHandleCandidates, candidate => Assert.Equal(deliveryId, candidate));
    }

    [Theory]
    [InlineData("uppercase")] // the handle format only accepts lowercase hexadecimal characters
    [InlineData("dashed")] // the handle format is the N format of a GUID, without dashes
    public async Task GetTrackSegment_WhenDeliveryIdFormatIsInvalid_ShouldReturnValidationProblem(string malformedHandleKind)
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string trackPath = WriteContentFile("love-of-my-life.ogg", CreateAudioBytes());
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        string validDeliveryId = await GetDeliveryIdAsync(libraryId, artistId, albumId, trackId);
        string malformedDeliveryId = malformedHandleKind switch
        {
            "uppercase" => validDeliveryId.ToUpperInvariant(),
            _ => Guid.Parse(validDeliveryId).ToString("D"),
        };

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(libraryId, artistId, albumId, trackId, malformedDeliveryId, 0));

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails.RootElement.GetProperty("detail").GetString());
        Assert.Contains(
            "DeliveryIdIsInvalid",
            problemDetails.RootElement.GetProperty("errors").GetProperty("General.Validation").EnumerateArray().Select(error => error.GetString()));
    }

    [Fact]
    public async Task GetTrackSegment_WhenDeliveryIdIsUnknown_ShouldReturnPlaybackDeliveryNotFound()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string trackPath = WriteContentFile("you-re-my-best-friend.ogg", CreateAudioBytes());
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        string unknownDeliveryId = Guid.NewGuid().ToString("N");

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(libraryId, artistId, albumId, trackId, unknownDeliveryId, 0));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = await ReadProblemDetailsAsync(response);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("PlaybackDeliveryNotFound", problemDetails.Detail);
    }

    [Theory]
    [InlineData("library")] // the handle was issued for the library of another delivery
    [InlineData("artist")] // the handle was issued for the artist of another delivery
    [InlineData("album")] // the handle was issued for the album of another delivery
    public async Task GetTrackSegment_WhenDeliveryWasIssuedForAnotherScope_ShouldReturnPlaybackDeliveryNotFound(string mismatchedScope)
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string trackPath = WriteContentFile("news-of-the-world.ogg", CreateAudioBytes());
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        string deliveryId = await GetDeliveryIdAsync(libraryId, artistId, albumId, trackId);
        // A second library owned by the same user keeps the ownership check passing, so the delivery scope is what rejects the request.
        Guid otherLibraryId = await SeedLibraryAsync(userId);
        Guid requestedLibraryId = mismatchedScope == "library" ? otherLibraryId : libraryId;
        Guid requestedArtistId = mismatchedScope == "artist" ? Guid.NewGuid() : artistId;
        Guid requestedAlbumId = mismatchedScope == "album" ? Guid.NewGuid() : albumId;

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(requestedLibraryId, requestedArtistId, requestedAlbumId, trackId, deliveryId, 0));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = await ReadProblemDetailsAsync(response);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("PlaybackDeliveryNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackSegment_WhenTrackFileIsOutsideTheLibraryContentLocations_ShouldReturnTrackFileNotFoundWithoutLeakingThePath()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string outsideFilePath = Path.Combine(Path.GetTempPath(), $"lumina-track-segment-outside-{Guid.NewGuid():N}.ogg");
        await File.WriteAllBytesAsync(outsideFilePath, CreateAudioBytes());
        _temporaryFiles.Add(outsideFilePath);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, outsideFilePath);
        string deliveryId = await GetDeliveryIdAsync(libraryId, artistId, albumId, trackId);

        // Act
        HttpResponseMessage response = await _client.GetAsync(BuildSegmentPath(libraryId, artistId, albumId, trackId, deliveryId, 0));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(outsideFilePath, content, StringComparison.OrdinalIgnoreCase);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackFileNotFound", problemDetails.Detail);
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="trackPath">The file system path of the audio file of the track.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId, string trackPath)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: [_contentRootPath]));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            path: trackPath,
            container: "OGG",
            audioCodec: "OPUS",
            title: "A Track",
            includeMetadata: false));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        _seededArtistIds.Add(artistId);
        _seededAlbumIds.Add(albumId);
        _seededTrackIds.Add(trackId);
        return (libraryId, artistId, albumId, trackId);
    }

    /// <summary>
    /// Seeds an additional music library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Reissues", libraryType: LibraryType.Music, contentLocations: [_contentRootPath]));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        return libraryId;
    }

    /// <summary>
    /// Requests a transcoded playback manifest for the seeded track and returns the delivery handle it reports.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The delivery handle of the normalized delivery.</returns>
    private async Task<string> GetDeliveryIdAsync(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest?capabilities=mp3/mp3");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        PlaybackManifestResponse? manifest = JsonSerializer.Deserialize<PlaybackManifestResponse>(content, _jsonOptions);
        Assert.NotNull(manifest);
        Assert.NotNull(manifest!.DeliveryId);
        return manifest.DeliveryId;
    }

    /// <summary>
    /// Builds the route of a single segment of the normalized audio of a track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <param name="deliveryId">The opaque handle of the normalized delivery.</param>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <returns>The route of the requested segment.</returns>
    private static string BuildSegmentPath(Guid libraryId, Guid artistId, Guid albumId, Guid trackId, string deliveryId, int segmentIndex)
    {
        return $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream/deliveries/{deliveryId}/segments/{segmentIndex}";
    }

    /// <summary>
    /// Writes an audio file with the provided <paramref name="audioBytes"/> inside the content root of the test, and returns its full path.
    /// </summary>
    /// <param name="fileName">The name of the audio file to write.</param>
    /// <param name="audioBytes">The bytes of the audio file to write.</param>
    /// <returns>The full path of the written audio file.</returns>
    private string WriteContentFile(string fileName, byte[] audioBytes)
    {
        string filePath = Path.Combine(_contentRootPath, fileName);
        File.WriteAllBytes(filePath, audioBytes);
        return filePath;
    }

    /// <summary>
    /// Creates a deterministic byte array that stands in for the content of an audio file.
    /// </summary>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateAudioBytes()
    {
        return [.. Enumerable.Range(0, 4096).Select(index => (byte)(index % 256))];
    }

    /// <summary>
    /// Reads the problem details of a response.
    /// </summary>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized problem details.</returns>
    private async Task<Microsoft.AspNetCore.Mvc.ProblemDetails?> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
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
    /// Cleans up the seeded database rows, the temporary files and the test user of the test.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            foreach (Guid trackId in _seededTrackIds)
            {
                TrackEntity? track = dbContext.Tracks.Find(trackId);
                if (track is not null)
                    dbContext.Tracks.Remove(track);
            }
            foreach (Guid albumId in _seededAlbumIds)
            {
                AlbumEntity? album = dbContext.Albums.Find(albumId);
                if (album is not null)
                    dbContext.Albums.Remove(album);
            }
            foreach (Guid artistId in _seededArtistIds)
            {
                ArtistEntity? artist = dbContext.Artists.Find(artistId);
                if (artist is not null)
                    dbContext.Artists.Remove(artist);
            }
            foreach (Guid libraryId in _seededLibraryIds)
            {
                LibraryEntity? library = dbContext.Libraries.Find(libraryId);
                if (library is not null)
                    dbContext.Libraries.Remove(library);
            }
            await dbContext.SaveChangesAsync();
        }
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, recursive: true);
        foreach (string temporaryFile in _temporaryFiles)
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        await _apiFactory.RemoveTestUserAsync();
    }
}
