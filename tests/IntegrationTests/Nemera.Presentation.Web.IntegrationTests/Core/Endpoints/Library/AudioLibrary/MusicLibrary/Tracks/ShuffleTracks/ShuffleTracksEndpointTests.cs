#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Contains integration tests for the <see cref="ShuffleTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string SHUFFLE_TRACKS_PATH = "/en-us/library/audio-library/music-library/tracks/api-shuffle-tracks";
    private readonly LuminaWebFactory _apiFactory;
    private readonly ShuffleTracksRequestFixture _shuffleTracksRequestFixture = new();
    private readonly MusicShuffleTrackDtoFixture _musicShuffleTrackDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ShuffleTracksEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ShuffleTracks_WhenApiReturnsTracks_ShouldReturnTheMappedTracks()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid trackId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid excludedTrackId = Guid.NewGuid();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(
            count: 25, excludedTrackIds: [excludedTrackId]);
        MusicShuffleTrackDto expectedTrack = _musicShuffleTrackDtoFixture.Create(
            trackId: trackId,
            title: "Bohemian Rhapsody",
            durationInSeconds: 355,
            albumId: albumId,
            albumTitle: "A Night at the Opera",
            artistId: artistId,
            coverPath: "a-night-at-the-opera-cover.jpg");
        _apiFactory.ApiClientStub.RegisterPostResponse(ExpectedEndpoint(request.LibraryId), new List<MusicShuffleTrackDto> { expectedTrack });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateShuffleRequest(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetArrayLength());
        JsonElement track = data[0];
        Assert.Equal(trackId, track.GetProperty("trackId").GetGuid());
        Assert.Equal("Bohemian Rhapsody", track.GetProperty("title").GetString());
        Assert.Equal(355, track.GetProperty("durationInSeconds").GetInt32());
        Assert.Equal(albumId, track.GetProperty("albumId").GetGuid());
        Assert.Equal("A Night at the Opera", track.GetProperty("albumTitle").GetString());
        Assert.Equal(artistId, track.GetProperty("artistId").GetGuid());
        Assert.Equal("a-night-at-the-opera-cover.jpg", track.GetProperty("coverPath").GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShuffleTracks_WhenCalled_ShouldForwardTheRequestToTheLibraryShuffleEndpoint()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid firstExcludedTrackId = Guid.NewGuid();
        Guid secondExcludedTrackId = Guid.NewGuid();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(
            count: 25,
            artistId: artistId,
            albumId: albumId,
            excludedTrackIds: [firstExcludedTrackId, secondExcludedTrackId]);
        _apiFactory.ApiClientStub.RegisterPostResponse(ExpectedEndpoint(request.LibraryId), new List<MusicShuffleTrackDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateShuffleRequest(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        (string Endpoint, object? Data) forwardedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint == ExpectedEndpoint(request.LibraryId));
        Assert.Equal($"libraries/{request.LibraryId}/tracks/shuffle", forwardedRequest.Endpoint);
        ShuffleTracksRequest forwarded = Assert.IsType<ShuffleTracksRequest>(forwardedRequest.Data);
        Assert.Equal(request.LibraryId, forwarded.LibraryId);
        Assert.Equal(25, forwarded.Count);
        Assert.Equal(artistId, forwarded.ArtistId);
        Assert.Equal(albumId, forwarded.AlbumId);
        Assert.NotNull(forwarded.ExcludedTrackIds);
        Assert.Equal(new[] { firstExcludedTrackId, secondExcludedTrackId }, forwarded.ExcludedTrackIds);
    }

    [Fact]
    public async Task ShuffleTracks_WhenApiReturnsNoTracks_ShouldReturnAnEmptyDataArray()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostResponse(ExpectedEndpoint(request.LibraryId), new List<MusicShuffleTrackDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateShuffleRequest(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.Equal(0, data.GetArrayLength());
    }

    [Fact]
    public async Task ShuffleTracks_WhenBodyIsMalformed_ShouldNotLeakServerDetailsOrCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        StringContent body = new("{\"libraryId\": \"'; DROP TABLE Tracks; --\", \"count\": \"'; DROP TABLE Tracks; --\"}", Encoding.UTF8, "application/json");
        HttpRequestMessage shuffleRequest = new(HttpMethod.Post, SHUFFLE_TRACKS_PATH)
        {
            Content = body
        };
        shuffleRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        shuffleRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(shuffleRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The unbound request is rejected before the upstream API is reached, and the JSON error is sanitized.
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.EndsWith("/tracks/shuffle", StringComparison.Ordinal));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("'; DROP TABLE Tracks; --", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Builds the upstream API endpoint of the shuffle request for the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the tracks are sampled from.</param>
    /// <returns>The upstream endpoint.</returns>
    private static string ExpectedEndpoint(Guid libraryId)
    {
        return ApiRoutes.Music.SHUFFLE_TRACKS.Replace("{libraryId}", libraryId.ToString());
    }

    /// <summary>
    /// Builds the Web shuffle request that posts the given <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The shuffle request to send.</param>
    /// <returns>The configured Web request.</returns>
    private static HttpRequestMessage CreateShuffleRequest(ShuffleTracksRequest request)
    {
        HttpRequestMessage shuffleRequest = new(HttpMethod.Post, SHUFFLE_TRACKS_PATH)
        {
            Content = JsonContent.Create(request)
        };
        shuffleRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        shuffleRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return shuffleRequest;
    }
}
