#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaybackManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPlaybackManifestEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client of the test.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenTrackBelongsToAnOwnedLibrary_ShouldReturnDirectPlayManifest()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(
            userId,
            container: "FLAC",
            audioCodec: "FLAC",
            replayGainTrackGain: -7.5m,
            replayGainTrackPeak: 0.9877m,
            replayGainAlbumGain: -6.8m,
            replayGainAlbumPeak: 0.9999m);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaybackManifestResponse? manifest = await ReadManifestAsync(response);
        Assert.NotNull(manifest);
        Assert.Equal(trackId, manifest!.TrackId);
        Assert.True(manifest.DirectPlay);
        Assert.Equal("FLAC", manifest.SourceContainer);
        Assert.Equal("FLAC", manifest.SourceCodec);
        Assert.Equal(-7.5m, manifest.ReplayGainTrackGain);
        Assert.Equal(0.9877m, manifest.ReplayGainTrackPeak);
        Assert.Equal(-6.8m, manifest.ReplayGainAlbumGain);
        Assert.Equal(0.9999m, manifest.ReplayGainAlbumPeak);
        Assert.Null(manifest.DeliveryContainer);
        Assert.Null(manifest.DeliveryCodec);
        Assert.Null(manifest.DeliveryShape);
        Assert.Null(manifest.SegmentDurationSeconds);
        Assert.True(manifest.Seekable);
        Assert.Equal($"/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream", manifest.StreamPath);
        Assert.DoesNotContain("http", manifest.StreamPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenPlayerCannotDecodeTheStoredCodec_ShouldReturnTranscodedDelivery()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, container: "OGG", audioCodec: "OPUS");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest?capabilities=mp3/mp3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaybackManifestResponse? manifest = await ReadManifestAsync(response);
        Assert.NotNull(manifest);
        Assert.Equal(trackId, manifest!.TrackId);
        Assert.False(manifest.DirectPlay);
        Assert.Equal("MP3", manifest.DeliveryContainer);
        Assert.Equal("MP3", manifest.DeliveryCodec);
        Assert.Equal(PlaybackDeliveryShape.TimeAddressedSegments, manifest.DeliveryShape);
        Assert.Equal(6, manifest.SegmentDurationSeconds);
        Assert.True(manifest.Seekable);
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenTrackIsRequestedThroughDifferentAlbum_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid artistId, _, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, container: "FLAC", audioCodec: "FLAC");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{Guid.NewGuid()}/tracks/{trackId}/playback-manifest");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = await ReadProblemDetailsAsync(response);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenTrackBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, container: "FLAC", audioCodec: "FLAC");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.Unauthorized", problemDetails!["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="container">The container of the audio file of the seeded track.</param>
    /// <param name="audioCodec">The codec of the audio of the seeded track.</param>
    /// <param name="replayGainTrackGain">Optional. The ReplayGain track gain of the seeded track, in decibels.</param>
    /// <param name="replayGainTrackPeak">Optional. The ReplayGain track peak of the seeded track.</param>
    /// <param name="replayGainAlbumGain">Optional. The ReplayGain album gain of the seeded track, in decibels.</param>
    /// <param name="replayGainAlbumPeak">Optional. The ReplayGain album peak of the seeded track.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(
        Guid userId,
        string container,
        string audioCodec,
        decimal? replayGainTrackGain = null,
        decimal? replayGainTrackPeak = null,
        decimal? replayGainAlbumGain = null,
        decimal? replayGainAlbumPeak = null)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            container: container,
            audioCodec: audioCodec,
            title: "A Track",
            includeMetadata: false,
            replayGainTrackGain: replayGainTrackGain,
            replayGainTrackPeak: replayGainTrackPeak,
            replayGainAlbumGain: replayGainAlbumGain,
            replayGainAlbumPeak: replayGainAlbumPeak));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId, trackId);
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
    /// Reads the playback manifest of a response.
    /// </summary>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized playback manifest.</returns>
    private async Task<PlaybackManifestResponse?> ReadManifestAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PlaybackManifestResponse>(content, _jsonOptions);
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
    /// Removes the test user created for the tests.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
